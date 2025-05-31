using Auth.Application.Interfaces;
using Auth.Infrastructure.Data;
using Auth.Infrastructure.Extensions;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using System.Text; // Add this import at the top of the file

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog for structured logging
Log.Logger = new LoggerConfiguration()
	.ReadFrom.Configuration(builder.Configuration) // Read configuration from appsettings.json
	.Enrich.FromLogContext() // Enrich logs with context information
	.CreateLogger();

builder.Host.UseSerilog(); // Use Serilog as the logging provider

// Add services to the container.

// Register infrastructure services (DbContext, Identity, Repositories, etc.)
// This extension method is defined in the Auth.Infrastructure project.
builder.Services.AddAuthInfrastructure(builder.Configuration);

// Register application services - IAuthService and its implementation AuthService
// Scoped lifetime means an instance is created once per client request.
builder.Services.AddScoped<IAuthService, Auth.Application.Services.AuthService>();


// Configure JWT Authentication
// Retrieve JWT settings from appsettings.json
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var secretKey = jwtSettings["SecretKey"] ?? throw new InvalidOperationException("Missing JwtSettings:SecretKey in configuration.");
var issuer = jwtSettings["Issuer"] ?? throw new InvalidOperationException("Missing JwtSettings:Issuer in configuration.");
var audience = jwtSettings["Audience"] ?? throw new InvalidOperationException("Missing JwtSettings:Audience in configuration.");

// Add Authentication services and configure JWT Bearer as the default scheme
builder.Services.AddAuthentication(options =>
{
	options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme; // Scheme for authenticating users
	options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;    // Scheme for challenging unauthenticated users
})
.AddJwtBearer(options => // Configure JWT Bearer authentication handler
{
	options.RequireHttpsMetadata = false; // For development; set to true in production environments
	options.SaveToken = true;             // Save the validated token in the AuthenticationProperties
	options.TokenValidationParameters = new TokenValidationParameters
	{
		ValidateIssuer = true, // Validate the server that generates the token
		ValidIssuer = issuer,  // Expected issuer of the token

		ValidateAudience = true, // Validate the recipient of the token (this service)
		ValidAudience = audience, // Expected audience

		ValidateIssuerSigningKey = true, // Validate the signature of the token
		IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)), // Key used to sign the token

		ValidateLifetime = true, // Validate that the token is not expired and that the signing key is valid
		ClockSkew = TimeSpan.Zero // No tolerance for clock differences between server and client
	};
});

// Add Authorization services
builder.Services.AddAuthorization();

// Add Controllers services to handle API requests
builder.Services.AddControllers();

// Configure Swagger/OpenAPI for API documentation and testing UI
builder.Services.AddEndpointsApiExplorer(); // Required for Swagger with minimal APIs/controllers
builder.Services.AddSwaggerGen();          // Adds Swagger generation services

var app = builder.Build(); // Build the WebApplication instance

// Configure the HTTP request pipeline (middleware).
// Order of middleware is important.

// In development environment, enable Swagger and Swagger UI
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();       // Serves the Swagger JSON specification
    app.UseSwaggerUI();     // Serves the Swagger UI HTML page
}


using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AuthDbContext>();
    dbContext.Database.Migrate(); // This will apply pending migrations
}

// Redirect HTTP requests to HTTPS for security
app.UseHttpsRedirection();

// Add Serilog request logging middleware for rich, structured logs of HTTP requests
app.UseSerilogRequestLogging();

// Enable routing to map requests to endpoints
app.UseRouting();

// Enable Authentication middleware - attempts to authenticate users based on the configured schemes (JWT)
app.UseAuthentication();
// Enable Authorization middleware - enforces authorization policies on endpoints
app.UseAuthorization();

// Map controller endpoints to the request pipeline
app.MapControllers();

// Run the application
app.Run();
