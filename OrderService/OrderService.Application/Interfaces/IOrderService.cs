using Shared.Application.DTOs;

namespace OrderService.Application.Interfaces
{
	public interface IOrderService
	{
		Task<IEnumerable<OrderDto>> GetAllByEmployeeAsync(Guid employeeId);
		Task<OrderDto?> GetByIdAsync(Guid id);
		Task<OrderDto> CreateAsync(OrderDto dto);
		Task UpdateAsync(OrderDto dto);
		Task DeleteAsync(Guid id);
	}
}
