using Shared.Application.DTOs;
using ProductService.Application.Interfaces;
using ProductService.Domain.Entities;
using ProductService.Domain.Interfaces;

namespace ProductService.Application.Services;

public class ProductService(IProductRepository repository) : IProductService
{
	public async Task<IEnumerable<ProductDto>> GetAllAsync()
	{
		var products = await repository.GetAllAsync();
		return products.Select(p => new ProductDto { Id = p.Id, Name = p.Name, Price = p.Price });
	}

	public async Task<ProductDto?> GetByIdAsync(Guid id)
	{
		var product = await repository.GetByIdAsync(id);
		if (product == null) return null;
		return new ProductDto { Id = product.Id, Name = product.Name, Price = product.Price };
	}

	public async Task<ProductDto> CreateAsync(ProductDto dto)
	{
		var product = new Product(Guid.NewGuid(), dto.Name, dto.Price);
		await repository.AddAsync(product);
		return new ProductDto { Id = product.Id, Name = product.Name, Price = product.Price };
	}

	public async Task UpdateAsync(ProductDto dto)
	{
		var existing = await repository.GetByIdAsync(dto.Id);
		if (existing == null)
			throw new KeyNotFoundException($"Product with ID {dto.Id} not found.");

		existing.Update(dto.Name, dto.Price);
		await repository.UpdateAsync(existing);
	}

	public async Task DeleteAsync(Guid id)
	{
		await repository.DeleteAsync(id);
	}
}
