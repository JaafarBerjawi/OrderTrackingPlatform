using Shared.Application.DTOs;

namespace ProductService.Application.Interfaces;

public interface IProductService
{
	Task<IEnumerable<ProductDto>> GetAllAsync();
	Task<ProductDto?> GetByIdAsync(Guid id);
	Task<ProductDto> CreateAsync(ProductDto dto);
	Task UpdateAsync(ProductDto dto);
	Task DeleteAsync(Guid id);
}