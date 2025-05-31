using Serilog.Events;
using Serilog;
using OrderService.Infrastructure.Extensions;

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

// Register infrastructure services (DbContext, Repositories like IOrderRepository),
// application services (like IOrderService), and potentially HttpClient configurations (e.g., for ProductService)
// using the extension method from OrderService.Infrastructure.
// This promotes a clean separation of concerns in service registration.
builder.Services.AddOrderInfrastructure(builder.Configuration);

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
// This service likely relies on authorization decisions (e.g., user identity from X-Employee-Id header)
// made by the API Gateway or passed through it.
app.UseAuthorization();

// Map controller endpoints to the request pipeline
app.MapControllers();

// Map the health check endpoint to "/health"
// This allows monitoring systems to check the service's health status.
app.MapHealthChecks("/health");

// Run the application
app.Run();
