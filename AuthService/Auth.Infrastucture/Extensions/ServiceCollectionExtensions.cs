using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Auth.Infrastructure.Repositories;
using Auth.Domain.Interfaces;
using Auth.Infrastructure.Data;

namespace Auth.Infrastructure.Extensions
{
	public static class ServiceCollectionExtensions
	{
		public static IServiceCollection AddAuthInfrastructure(this IServiceCollection services, IConfiguration configuration)
		{
			// 1) Configure AuthDbContext (PostgreSQL example; swap for SQL Server if needed)
			services.AddDbContext<AuthDbContext>(options =>
				options.UseNpgsql(configuration.GetConnectionString("AuthDatabase")));

			// 2) Register repository
			services.AddScoped<IUserRepository, UserRepository>();

			return services;
		}
	}
}
