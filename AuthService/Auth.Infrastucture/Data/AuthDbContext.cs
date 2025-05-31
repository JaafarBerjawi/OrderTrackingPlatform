using Auth.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Auth.Infrastructure.Data;

public class AuthDbContext : DbContext
{
	public AuthDbContext(DbContextOptions<AuthDbContext> options)
		: base(options)
	{ }
	public DbSet<User> Users { get; set; } = null!;

	protected override void OnModelCreating(ModelBuilder modelBuilder)
	{
		base.OnModelCreating(modelBuilder);

		modelBuilder.Entity<User>(entity =>
		{
			entity.ToTable("Users", "sec");
			entity.HasKey(u => u.Id);
			entity.Property(u => u.Username)
				  .IsRequired()
				  .HasMaxLength(100);
			entity.Property(u => u.PasswordHash)
				  .IsRequired()
				  .HasMaxLength(200);
		});
	}
}
