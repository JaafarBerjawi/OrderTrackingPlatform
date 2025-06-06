﻿using ProductService.Domain.Entities;

namespace ProductService.Domain.Interfaces
{
	public interface IProductRepository
	{
		Task<Product?> GetByIdAsync(Guid id);
		Task<IEnumerable<Product>> GetAllAsync();
		Task AddAsync(Product product);
		Task UpdateAsync(Product product);
		Task DeleteAsync(Guid id);
	}
}