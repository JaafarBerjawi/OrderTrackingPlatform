using Microsoft.EntityFrameworkCore;
using ProductService.Domain.Entities;

namespace ProductService.Infrastructure.Data
{
	public class ProductDbContext : DbContext
	{
		private readonly string _schema = "product";
		public ProductDbContext(DbContextOptions<ProductDbContext> options)
			: base(options)
		{
		}

		public DbSet<Product> Products { get; set; } = null!;

		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			base.OnModelCreating(modelBuilder);

			modelBuilder.Entity<Product>(entity =>
			{
				entity.ToTable("Products", _schema);
				entity.HasKey(p => p.Id);
				entity.Property(p => p.Name).IsRequired().HasMaxLength(200);
				entity.Property(p => p.Price).HasColumnType("decimal(18,2)");
			});
		}
	}
}