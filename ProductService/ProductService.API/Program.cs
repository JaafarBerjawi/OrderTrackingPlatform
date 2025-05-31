using ProductService.Infrastructure.Extensions;
using Serilog;
using Serilog.Events;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog for structured logging
Log.Logger = new LoggerConfiguration()
	.MinimumLevel.Override("Microsoft", LogEventLevel.Information) // Optionally override log levels for specific sources
	.Enrich.FromLogContext() // Enrich logs with context information
	.WriteTo.Console()       // Write logs to the console
	.CreateLogger();

builder.Host.UseSerilog(); // Use Serilog as the logging provider

// Add services to the container.

// Add Controllers services to handle API requests
builder.Services.AddControllers();

// Register infrastructure services (DbContext, Repositories like IProductRepository)
// and application services (like IProductService) using the extension method from ProductService.Infrastructure.
// This promotes a clean separation of concerns in service registration.
builder.Services.AddProductInfrastructure(builder.Configuration);

// Add Health Check services to report the service status
builder.Services.AddHealthChecks();

// Configure Swagger/OpenAPI for API documentation and testing UI
builder.Services.AddEndpointsApiExplorer(); // Required for Swagger with minimal APIs/controllers
builder.Services.AddSwaggerGen();          // Adds Swagger generation services

var app = builder.Build(); // Build the WebApplication instance

// Configure the HTTP request pipeline (middleware).
// The order of middleware registration is important.

// In development environment, enable Swagger and Swagger UI for API testing and documentation
if (app.Environment.IsDevelopment())
{
	app.UseSwagger();       // Serves the Swagger JSON specification (api-docs)
	app.UseSwaggerUI();     // Serves the Swagger UI HTML page
}

// Redirect HTTP requests to HTTPS for enhanced security
app.UseHttpsRedirection();

// Enable Authorization middleware.
// Note: Specific authorization policies or JWT validation are not configured here directly.
// This service might rely on authorization decisions made by the API Gateway,
// or use simple [Authorize] attributes if specific local policies were added.
app.UseAuthorization();

// Map controller endpoints to the request pipeline
app.MapControllers();

// Map the health check endpoint to "/health"
// This allows monitoring systems to check the service's health status.
app.MapHealthChecks("/health");

// Run the application
app.Run();
