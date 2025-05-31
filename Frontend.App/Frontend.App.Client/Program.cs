using Login.Business.Interfaces;
using Login.Business.Services;
using Login.Web.Middlewares; // Contains AuthenticationMiddleware
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.IdentityModel.Tokens;
using Order.Application.Interfaces;
using Order.Application.Services;
using Shared.Infrastructure.Clients; // Contains DefaultHttpClient
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Configure localization services, specifying the path to resource files
builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");

// Configure Razor Pages services
builder.Services.AddRazorPages()
	// Enable localization for Razor views
	.AddViewLocalization()
	// Enable localization for data annotations (validation messages)
	.AddDataAnnotationsLocalization()
	// Add application parts from external assemblies to discover Razor Pages, views, etc.
	// This allows Login.Web and Order.Web (presumably class libraries containing Razor Pages) to be part of this web app.
	.AddApplicationPart(typeof(Login.Web.Pages.LoginModel).Assembly)
	.AddApplicationPart(typeof(Order.Web.Pages.OrderModel).Assembly);

// Add session services to enable session state management
builder.Services.AddSession();
// Add HttpContextAccessor to allow access to HttpContext from services
builder.Services.AddHttpContextAccessor();

builder.Services.AddDataProtection()
	.PersistKeysToFileSystem(new DirectoryInfo("/root/.aspnet/DataProtection-Keys"))
	.SetApplicationName("OrderTrackingFrontend");


// Configure a typed HttpClient "DefaultClient" for making requests to backend APIs
builder.Services.AddHttpClient<DefaultHttpClient>("DefaultClient", client =>
{
	// Set the base address for the HttpClient from configuration (e.g., API Gateway URL for authentication)
	client.BaseAddress = new Uri(builder.Configuration["ClientAPIs:Auth"]
	                             ?? throw new InvalidOperationException("ClientAPIs:Auth configuration is missing."));
});


// Retrieve JWT settings from configuration for token validation
var issuer = builder.Configuration["JwtSettings:Issuer"] ?? throw new InvalidOperationException("JwtSettings:Issuer configuration is missing.");
var audience = builder.Configuration["JwtSettings:Audience"] ?? throw new InvalidOperationException("JwtSettings:Audience configuration is missing.");
var secret = builder.Configuration["JwtSettings:SecretKey"] ?? throw new InvalidOperationException("JwtSettings:SecretKey configuration is missing.");

// Configure JWT Bearer authentication
builder.Services.AddAuthentication(options =>
{
	// Set default schemes for authentication and challenge
	options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
	options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
	.AddJwtBearer(options => // Configure JWT Bearer handler
	{
		// Define custom event handlers for JWT authentication events
		options.Events = new JwtBearerEvents
		{
			// This event is triggered when a message is received that might contain a token
			OnMessageReceived = context =>
			{
				// Attempt to retrieve the JWT token from a cookie named "access_token"
				var token = context.Request.Cookies["access_token"];
				context.Token = token; // Set the token on the context for validation
				return Task.CompletedTask;
			},
			// This event is triggered when an unauthenticated user tries to access a protected resource
			OnChallenge = context =>
			{
				context.HandleResponse(); // Skip the default challenge logic
				context.Response.Redirect("/Login"); // Redirect the user to the Login page
				return Task.CompletedTask;
			},
			// This event is triggered if authentication fails
			OnAuthenticationFailed = context =>
			{
				// Specifically handle token expiration
				if (context.Exception.GetType() == typeof(SecurityTokenExpiredException))
				{
					context.Response.StatusCode = StatusCodes.Status401Unauthorized; // Set 401 status
					context.Response.Cookies.Delete("access_token"); // Delete the expired token cookie
					context.Response.Redirect("/Login"); // Redirect to Login page
					// context.HandleResponse(); // Consider if default behavior should be skipped
				}
				return Task.CompletedTask;
			},
		};

		// Configure token validation parameters
		options.TokenValidationParameters = new TokenValidationParameters
		{
			ValidateIssuer = true, // Validate the token issuer
			ValidIssuer = issuer,  // Expected issuer
			ValidateAudience = true, // Validate the token audience
			ValidAudience = audience, // Expected audience
			ValidateLifetime = true, // Validate the token's expiration
			ValidateIssuerSigningKey = true, // Validate the signing key
			IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret)) // Signing key
		};
	});

// Register application-specific services as singletons
// ILoginService and IOrderService are business logic services.
builder.Services.AddSingleton<ILoginService, LoginService>();
builder.Services.AddSingleton<IOrderService, OrderService>();

var app = builder.Build(); // Build the WebApplication

// Configure the HTTP request pipeline (middleware).

// For non-development environments, configure custom error handling and HSTS
if (!app.Environment.IsDevelopment())
{
	app.UseExceptionHandler("/Error"); // Use a generic error handler page
	// The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
	app.UseHsts(); // Add HSTS headers for security
}

app.UseHttpsRedirection(); // Redirect HTTP requests to HTTPS
app.UseStaticFiles();      // Enable serving static files (CSS, JavaScript, images)

app.UseRouting();          // Enable routing to map requests to endpoints

app.UseSession();          // Enable session state

app.UseAuthentication();   // Enable authentication middleware
app.UseAuthorization();    // Enable authorization middleware

// Configure request localization based on supported cultures
var supportedCultures = new[] { "en", "ar" };
var localizationOptions = new RequestLocalizationOptions()
	.SetDefaultCulture(supportedCultures[0]) // Default culture "en"
	.AddSupportedCultures(supportedCultures)   // Add "en" and "ar" as supported cultures
	.AddSupportedUICultures(supportedCultures); // Add "en" and "ar" as supported UI cultures
app.UseRequestLocalization(localizationOptions); // Apply localization options

// Use custom authentication middleware (likely for additional auth logic or claims transformation)
app.UseMiddleware<AuthenticationMiddleware>();

app.MapRazorPages(); // Map Razor Pages endpoints

app.Run(); // Run the application
