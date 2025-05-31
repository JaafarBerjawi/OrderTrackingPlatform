using Shared.Application.DTOs;

namespace Order.Application.Interfaces
{
    public interface IOrderService
    {
        Task<OrderDto> GetOrderById(Guid id);
        Task<List<OrderDto>> GetOrders();
        Task<bool> CreateOrder(OrderDto orderDto);
        Task<bool> EditOrder(OrderDto orderDto);
        Task<bool> DeleteOrder(Guid id);
        Task<List<ProductDto>> GetProducts();

	}
}
