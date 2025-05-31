using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ProductService.Application.Interfaces;
using ProductService.Domain.Interfaces;
using ProductService.Infrastructure.Data;
using ProductService.Infrastructure.Repositories;

namespace ProductService.Infrastructure.Extensions
{
	public static class ServiceCollectionExtensions
	{
		public static IServiceCollection AddProductInfrastructure(this IServiceCollection services, IConfiguration configuration)
		{
			services.AddDbContext<ProductDbContext>(options =>
				options.UseNpgsql(configuration.GetConnectionString("ProductDatabase")));

			services.AddScoped<IProductRepository, ProductRepository>();
			services.AddScoped<IProductService, ProductService.Application.Services.ProductService>();

			return services;
		}
	}
}
