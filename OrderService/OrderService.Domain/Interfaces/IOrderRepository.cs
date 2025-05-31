// File: Domain/Interfaces/IOrderRepository.cs
using OrderService.Domain.Entities;

namespace OrderService.Domain.Interfaces
{
	public interface IOrderRepository
	{
		Task<Order?> GetByIdAsync(Guid id);
		Task<IEnumerable<Order>> GetAllByEmployeeAsync(Guid employeeId);
		Task AddAsync(Order order);
		Task UpdateAsync(Order order);
		Task DeleteAsync(Guid id);
	}
}