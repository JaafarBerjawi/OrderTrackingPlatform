using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using System.Text;
using Yarp.ReverseProxy.Transforms;
using System.IdentityModel.Tokens.Jwt;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog for structured logging
Log.Logger = new LoggerConfiguration()
	.ReadFrom.Configuration(builder.Configuration) // Read configuration from appsettings.json
	.Enrich.FromLogContext() // Enrich logs with context information
	.CreateLogger();
builder.Host.UseSerilog(); // Use Serilog as the logging provider

// Add services to the container.

// Retrieve JWT settings from configuration
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var secretKey = jwtSettings["SecretKey"];
var issuer = jwtSettings["Issuer"];
var audience = jwtSettings["Audience"];

// Configure JWT Authentication
builder.Services.AddAuthentication(options =>
{
	// Set default authentication schemes
	options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
	options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options => // Add JWT Bearer authentication handler
{
	options.RequireHttpsMetadata = false; // For development; set to true in production
	options.SaveToken = true; // Save the token in the HttpContext
	options.TokenValidationParameters = new TokenValidationParameters
	{
		ValidateIssuer = true, // Validate the token issuer
		ValidIssuer = issuer, // Expected issuer
		ValidateAudience = true, // Validate the token audience
		ValidAudience = audience, // Expected audience
		ValidateIssuerSigningKey = true, // Validate the signing key
		IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey!)), // Signing key
		ValidateLifetime = true, // Validate the token lifetime
		ClockSkew = TimeSpan.Zero // No clock skew tolerance
	};
});

// Configure Authorization services
builder.Services.AddAuthorization();

// Add MemoryCache service, used here for rate limiting
builder.Services.AddMemoryCache();

// Configure YARP (Yet Another Reverse Proxy)
builder.Services.AddReverseProxy()
	// Load reverse proxy configuration from the "ReverseProxy" section in appsettings.json
	.LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"))
	// Add custom transformations to the proxy requests
	.AddTransforms(transformBuilderContext =>
	{
		// This transform runs for every proxied request
		transformBuilderContext.AddRequestTransform(async transformContext =>
		{
			var httpContext = transformContext.HttpContext;
			// Check if the user is authenticated (meaning a valid JWT was presented to the gateway)
			if (httpContext.User.Identity?.IsAuthenticated == true)
			{
				// Extract the "sub" claim (typically the user ID) from the authenticated user's claims
				string? userId = httpContext.User.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Sub)?.Value;
				if (string.IsNullOrEmpty(userId)) // Fallback if 'sub' is not found, try the first claim (less specific)
				{
					userId = httpContext.User.Claims.First()?.Value;
				}

				if (!string.IsNullOrEmpty(userId))
				{
					// Add a custom header "X-Employee-Id" to the outgoing request to the downstream service
					// This allows downstream services to identify the user without needing to validate the JWT again
					transformContext.ProxyRequest.Headers.Add("X-Employee-Id", userId);
				}
			}
			await Task.CompletedTask; // Indicate completion of the asynchronous transform
		});
	});

// Configure Health Checks for the gateway and downstream services
builder.Services.AddHealthChecks()
	// Add a basic self-health check for the gateway itself
	.AddCheck("Self", () => Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckResult.Healthy())
	// Add health checks for downstream services by pinging their health endpoints
	.AddUrlGroup(new Uri(builder.Configuration["Downstream:ProductServiceHealth"]!), name: "ProductService")
	.AddUrlGroup(new Uri(builder.Configuration["Downstream:OrderServiceHealth"]!), name: "OrderService");

// Add controllers for handling direct API endpoints on the gateway (if any, e.g., health checks)
builder.Services.AddControllers();
// Configure Swagger/OpenAPI for API documentation and testing UI
builder.Services.AddEndpointsApiExplorer(); // Required for Swagger with minimal APIs
builder.Services.AddSwaggerGen(); // Adds Swagger generation services

var app = builder.Build(); // Build the WebApplication instance

// Configure the HTTP request pipeline.

// Use Serilog for rich, structured request logging
app.UseSerilogRequestLogging();

// Enable routing
app.UseRouting();

// Enable Authentication middleware - processes incoming JWTs
app.UseAuthentication();
// Enable Authorization middleware - enforces authorization policies
app.UseAuthorization();

// Custom Simple Rate Limiting Middleware
app.Use(async (context, next) =>
{
	// Get the MemoryCache service
	var memoryCache = context.RequestServices.GetRequiredService<IMemoryCache>();
	// Get client IP address, default to "unknown" if not available
	var clientIp = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
	// Create a key for the current minute to track requests per minute
	var currentMinute = DateTime.UtcNow.ToString("yyyyMMddHHmm");
	var cacheKey = $"RateLimit:{clientIp}:{currentMinute}";

	// Get or create a counter for the client IP for the current minute
	var count = memoryCache.GetOrCreate(cacheKey, entry =>
	{
		// Cache entry expires after 1 minute
		entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(1);
		return 0; // Initialize count to 0
	});

	// Get the rate limit from configuration
	var limit = int.Parse(builder.Configuration["RateLimiting:RequestsPerMinute"]!);
	// If the request count exceeds the limit
	if (count >= limit)
	{
		context.Response.StatusCode = StatusCodes.Status429TooManyRequests; // Return 429 Too Many Requests
		await context.Response.WriteAsync("Rate limit exceeded. Try again later.");
		return; // Stop further processing
	}
	// Increment the request count for the client IP and current minute
	memoryCache.Set(cacheKey, count + 1);
	await next(); // Pass control to the next middleware
});

// Map YARP reverse proxy routes and define a custom pipeline for it
app.MapReverseProxy(proxyPipeline =>
{
	// Custom middleware for the proxy pipeline
	proxyPipeline.Use(async (context, next) =>
	{
		// If the request path starts with "/api/auth", bypass JWT authentication
		// This allows unauthenticated access to login/register endpoints which are also proxied
		if (context.Request.Path.StartsWithSegments("/api/auth"))
		{
			await next(); // Proceed to the proxy
			return;
		}
		// For all other proxied routes, ensure the user is authenticated
		if (context.User.Identity == null || !context.User.Identity.IsAuthenticated)
		{
			context.Response.StatusCode = StatusCodes.Status401Unauthorized; // Return 401 Unauthorized
			return; // Stop further processing
		}
		await next(); // Proceed to the proxy
	});
});

// Map the health check endpoint for the gateway
app.MapHealthChecks("/gateway-health");

// Configure the HTTP request pipeline for development environment
if (app.Environment.IsDevelopment())
{
	// Enable Swagger middleware for API documentation
    app.UseSwagger();
	// Enable Swagger UI middleware for a browsable API documentation UI
    app.UseSwaggerUI();
}

// Redirect HTTP requests to HTTPS
app.UseHttpsRedirection();

// This UseAuthorization call is often redundant if controllers are not used extensively for protected resources on the gateway itself
// or if all authorization is handled by the reverse proxy pipeline or downstream services.
// However, it's standard practice to include it.
app.UseAuthorization();

// Map controller routes (e.g., for health checks or other direct gateway endpoints)
app.MapControllers();

// Run the application
app.Run();
