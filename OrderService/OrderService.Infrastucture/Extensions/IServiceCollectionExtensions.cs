using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OrderService.Application.Interfaces;
using OrderService.Domain.Interfaces;
using OrderService.Infrastructure.Data;
using OrderService.Infrastructure.Repositories;
using Serilog;

namespace OrderService.Infrastructure.Extensions
{
	public static class ServiceCollectionExtensions
	{
		public static IServiceCollection AddOrderInfrastructure(this IServiceCollection services, IConfigurationManager configuration)
		{
			// Configure DbContext (PostgreSQL example)
			services.AddDbContext<OrderDbContext>(options =>
				options.UseNpgsql(configuration.GetConnectionString("OrderDatabase")));

			// Register repository and application service
			services.AddScoped<IOrderRepository, OrderRepository>();
			services.AddScoped<IOrderService, OrderService.Application.Services.OrderService>();

			// Configure HttpClient for ProductService
			services.AddHttpClient("ProductService", client =>
			{
				client.BaseAddress = new Uri(configuration["Services:ProductServiceUrl"]);
				client.DefaultRequestHeaders.Accept.Clear();
				client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
			});

			return services;
		}
	}
}