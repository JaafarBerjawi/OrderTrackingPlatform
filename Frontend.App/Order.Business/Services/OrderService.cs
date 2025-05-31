using Order.Application.Interfaces;
using Shared.Application.DTOs;
using Shared.Infrastructure.Clients;
using System.Net.Http.Json;

namespace Order.Application.Services
{
    public class OrderService : IOrderService
    {
        public readonly DefaultHttpClient _httpClient;

        public OrderService(DefaultHttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<OrderDto> GetOrderById(Guid id)
        {
            var response = await _httpClient.GetAsync($"/api/orders/{id.ToString()}");

            if (response.IsSuccessStatusCode)
                return await response.Content.ReadFromJsonAsync<OrderDto>();

            return null;
        }

        public async Task<List<OrderDto>> GetOrders()
        {
            var response = await _httpClient.GetAsync("/api/orders");

            if (response.IsSuccessStatusCode)
                return await response.Content.ReadFromJsonAsync<List<OrderDto>>();

            return null;
        }

        public async Task<bool> CreateOrder(OrderDto orderDto)
        {
            orderDto.OrderDate = orderDto.OrderDate.ToUniversalTime();
            var response = await _httpClient.PostAsJsonAsync($"/api/orders", orderDto);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> EditOrder(OrderDto orderDto)
        {
            orderDto.OrderDate = orderDto.OrderDate.ToUniversalTime();
            var response = await _httpClient.PutAsJsonAsync($"/api/orders/{orderDto.Id.ToString()}", orderDto);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> DeleteOrder(Guid id)
        {
            var response = await _httpClient.DeleteAsync($"/api/orders/{id.ToString()}");
            return response.IsSuccessStatusCode;
        }

        //Shall be in a separate module
        public async Task<List<ProductDto>> GetProducts()
        {
			var response = await _httpClient.GetAsync("/api/products");

			if (response.IsSuccessStatusCode)
				return await response.Content.ReadFromJsonAsync<List<ProductDto>>();

			return null;
		}
    }
}
