using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using System.Text;
using Yarp.ReverseProxy.Transforms;
using System.IdentityModel.Tokens.Jwt;

var builder = WebApplication.CreateBuilder(args);

Log.Logger = new LoggerConfiguration()
	.ReadFrom.Configuration(builder.Configuration)
	.Enrich.FromLogContext()
	.CreateLogger();
builder.Host.UseSerilog();

// Add services to the container.

var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var secretKey = jwtSettings["SecretKey"];
var issuer = jwtSettings["Issuer"];
var audience = jwtSettings["Audience"];

builder.Services.AddAuthentication(options =>
{
	options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
	options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
	options.RequireHttpsMetadata = false;
	options.SaveToken = true;
	options.TokenValidationParameters = new TokenValidationParameters
	{
		ValidateIssuer = true,
		ValidIssuer = issuer,
		ValidateAudience = true,
		ValidAudience = audience,
		ValidateIssuerSigningKey = true,
		IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey!)),
		ValidateLifetime = true,
		ClockSkew = TimeSpan.Zero
	};
});

builder.Services.AddAuthorization();

builder.Services.AddMemoryCache();

builder.Services.AddReverseProxy()
	.LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"))
	.AddTransforms(transformBuilderContext =>
	{
		transformBuilderContext.AddRequestTransform(async transformContext =>
		{
			var httpContext = transformContext.HttpContext;
			// Only if the user is authenticated (JWT was validated by gateway)
			if (httpContext.User.Identity?.IsAuthenticated == true)
			{
				// Read the "sub" claim (user ID) from the JWT
				string? userId = httpContext.User.Claims.First()?.Value;
				if (!string.IsNullOrEmpty(userId))
				{
					// Inject "X-Employee-Id" header into the outgoing proxied HTTP request
					transformContext.ProxyRequest.Headers.Add("X-Employee-Id", userId);
				}
			}
			await Task.CompletedTask;
		});
	});

builder.Services.AddHealthChecks()
	.AddCheck("Self", () => Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckResult.Healthy())
	.AddUrlGroup(new Uri(builder.Configuration["Downstream:ProductServiceHealth"]!), name: "ProductService")
	.AddUrlGroup(new Uri(builder.Configuration["Downstream:OrderServiceHealth"]!), name: "OrderService");


builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();
app.UseSerilogRequestLogging();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

// Simple Rate Limiting Middleware
app.Use(async (context, next) =>
{
	var memoryCache = context.RequestServices.GetRequiredService<IMemoryCache>();
	var clientIp = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
	var currentMinute = DateTime.UtcNow.ToString("yyyyMMddHHmm");
	var cacheKey = $"RateLimit:{clientIp}:{currentMinute}";

	var count = memoryCache.GetOrCreate(cacheKey, entry =>
	{
		entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(1);
		return 0;
	});

	var limit = int.Parse(builder.Configuration["RateLimiting:RequestsPerMinute"]!);
	if (count >= limit)
	{
		context.Response.StatusCode = StatusCodes.Status429TooManyRequests;
		await context.Response.WriteAsync("Rate limit exceeded. Try again later.");
		return;
	}
	memoryCache.Set(cacheKey, count + 1);
	await next();
});

// Map the YARP reverse proxy (all configured routes)
app.MapReverseProxy(proxyPipeline =>
{
	// Skip JWT requirement for /api/auth/*
	proxyPipeline.Use(async (context, next) =>
	{
		if (context.Request.Path.StartsWithSegments("/api/auth"))
		{
			await next();
			return;
		}
		// Otherwise, ensure the user is authenticated
		if (!context.User.Identity.IsAuthenticated)
		{
			context.Response.StatusCode = StatusCodes.Status401Unauthorized;
			return;
		}
		await next();
	});
});

// Map Gateway health-check endpoint
app.MapHealthChecks("/gateway-health");

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
