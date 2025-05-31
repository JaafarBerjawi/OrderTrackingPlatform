using Microsoft.EntityFrameworkCore;
using OrderService.Domain.Entities;
using System.Collections.Generic;
using System.Reflection.Emit;

namespace OrderService.Infrastructure.Data
{
	public class OrderDbContext : DbContext
	{
		public OrderDbContext(DbContextOptions<OrderDbContext> options)
			: base(options)
		{
		}
		public DbSet<Order> Orders { get; set; } = null!;

		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			base.OnModelCreating(modelBuilder);

			modelBuilder.Entity<Order>(entity =>
			{
				entity.ToTable("Orders");
				entity.HasKey(o => o.Id);
				entity.Property(o => o.ProductId).IsRequired();
				entity.Property(o => o.Quantity).IsRequired();
				entity.Property(o => o.Total).HasColumnType("decimal(18,2)");
				entity.Property(o => o.ClientId).IsRequired();
				entity.Property(o => o.OrderDate).IsRequired();
				entity.Property(o => o.LoggedInEmployeeId).IsRequired();
			});
		}
	}
}