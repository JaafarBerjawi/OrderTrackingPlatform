using Login.Business.Interfaces;
using Login.Business.Services;
using Login.Web.Middlewares;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Order.Application.Interfaces;
using Order.Application.Services;
using Shared.Infrastructure.Clients;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");

builder.Services.AddRazorPages()
	.AddViewLocalization()
	.AddDataAnnotationsLocalization()
	.AddApplicationPart(typeof(Login.Web.Pages.LoginModel).Assembly)
	.AddApplicationPart(typeof(Order.Web.Pages.OrderModel).Assembly);

builder.Services.AddSession();
builder.Services.AddHttpContextAccessor();

builder.Services.AddHttpClient<DefaultHttpClient>("DefaultClient", client =>
{
	client.BaseAddress = new Uri(builder.Configuration["ClientAPIs:Auth"]);
});


var issuer = builder.Configuration["JwtSettings:Issuer"];
var audience = builder.Configuration["JwtSettings:Audience"];
var secret = builder.Configuration["JwtSettings:SecretKey"];
builder.Services.AddAuthentication(options =>
{
	options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
	options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
	.AddJwtBearer(options =>
	{
		options.Events = new JwtBearerEvents
		{
			OnMessageReceived = context =>
			{
				var token = context.Request.Cookies["access_token"];
				context.Token = token;
				return Task.CompletedTask;
			},
			OnChallenge = context =>
			{
				context.HandleResponse();
				context.Response.Redirect("/Login");
				return Task.CompletedTask;
			},
			OnAuthenticationFailed = context =>
			{
				if (context.Exception.GetType() == typeof(SecurityTokenExpiredException))
				{
					context.Response.StatusCode = 401;
					context.Response.Cookies.Delete("access_token");
					context.Response.Redirect("/Login");
					//context.HandleResponse(); // prevent default behavior
				}

				return Task.CompletedTask;
			},
		};

		options.TokenValidationParameters = new TokenValidationParameters
		{
			ValidateIssuer = true,
			ValidIssuer = issuer,
			ValidateAudience = true,
			ValidAudience = audience,
			ValidateLifetime = true,
			ValidateIssuerSigningKey = true,
			IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret))
		};
	});

builder.Services.AddSingleton<ILoginService, LoginService>();
builder.Services.AddSingleton<IOrderService, OrderService>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
	app.UseExceptionHandler("/Error");
	app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseSession();

app.UseAuthentication();
app.UseAuthorization();

var supportedCultures = new[] { "en", "ar" };
var localizationOptions = new RequestLocalizationOptions()
	.SetDefaultCulture("en")
	.AddSupportedCultures(supportedCultures)
	.AddSupportedUICultures(supportedCultures);
app.UseRequestLocalization(localizationOptions);

app.UseMiddleware<AuthenticationMiddleware>();

app.MapRazorPages();

app.Run();
