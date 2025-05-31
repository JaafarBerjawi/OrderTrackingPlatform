using Microsoft.EntityFrameworkCore;
using ProductService.Domain.Entities;
using ProductService.Domain.Interfaces;
using ProductService.Infrastructure.Data;

namespace ProductService.Infrastructure.Repositories
{
	public class ProductRepository(ProductDbContext _context) : IProductRepository
	{
		public async Task AddAsync(Product product)
		{
			await _context.Products.AddAsync(product);
			await _context.SaveChangesAsync();
		}

		public async Task DeleteAsync(Guid id)
		{
			var product = await _context.Products.FindAsync(id);
			if (product != null)
			{
				_context.Products.Remove(product);
				await _context.SaveChangesAsync();
			}
		}

		public async Task<IEnumerable<Product>> GetAllAsync()
		{
			return await _context.Products.AsNoTracking().ToListAsync();
		}

		public async Task<Product?> GetByIdAsync(Guid id)
		{
			return await _context.Products.AsNoTracking().FirstOrDefaultAsync(p => p.Id == id);
		}

		public async Task UpdateAsync(Product product)
		{
			_context.Products.Update(product);
			await _context.SaveChangesAsync();
		}
	}
}