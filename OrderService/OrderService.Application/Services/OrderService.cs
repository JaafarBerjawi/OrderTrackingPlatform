using OrderService.Application.Interfaces;
using OrderService.Domain.Entities;
using OrderService.Domain.Interfaces;
using Serilog;
using Shared.Application.DTOs;
using System.Text.Json;

namespace OrderService.Application.Services
{
	public class OrderService : IOrderService
	{
		private readonly IOrderRepository _repository;
		private readonly HttpClient _productHttpClient;
		private readonly ILogger _logger;

		public OrderService(IOrderRepository repository, IHttpClientFactory httpClientFactory)
		{
			_repository = repository;
			_productHttpClient = httpClientFactory.CreateClient("ProductService");
			_logger = Log.ForContext<OrderService>();
		}

		public async Task<IEnumerable<OrderDto>> GetAllByEmployeeAsync(Guid employeeId)
		{
			var orders = await _repository.GetAllByEmployeeAsync(employeeId);
			return orders.Select(o => new OrderDto
			{
				Id = o.Id,
				ProductId = o.ProductId,
				Quantity = o.Quantity,
				Total = o.Total,
				ClientId = o.ClientId,
				OrderDate = o.OrderDate,
				LoggedInEmployeeId = o.LoggedInEmployeeId
			});
		}

		public async Task<OrderDto?> GetByIdAsync(Guid id)
		{
			var order = await _repository.GetByIdAsync(id);
			if (order == null) return null;
			return new OrderDto
			{
				Id = order.Id,
				ProductId = order.ProductId,
				Quantity = order.Quantity,
				Total = order.Total,
				ClientId = order.ClientId,
				OrderDate = order.OrderDate,
				LoggedInEmployeeId = order.LoggedInEmployeeId
			};
		}

		public async Task<OrderDto> CreateAsync(OrderDto dto)
		{
			// Validate ProductId via Product Service
			var response = await _productHttpClient.GetAsync($"api/products/{dto.ProductId}");
			if (!response.IsSuccessStatusCode)
			{
				throw new ArgumentException($"Product with ID {dto.ProductId} not found.");
			}

			// Optionally retrieve product details if needed
			var productJson = await response.Content.ReadAsStringAsync();
			var product = JsonSerializer.Deserialize<ProductDto>(productJson, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
			if (product == null)
				throw new ArgumentException("Invalid product data from Product Service.");

			// Calculate total if not provided
			decimal total = dto.Quantity * product.Price;

			var order = new Order(Guid.NewGuid(), dto.ProductId, dto.Quantity, total, dto.ClientId, dto.OrderDate, dto.LoggedInEmployeeId);
			await _repository.AddAsync(order);

			_logger.Information("Order {OrderId} created by employee {EmployeeId} for product {ProductId}, quantity {Quantity}", order.Id, order.LoggedInEmployeeId, order.ProductId, order.Quantity);

			return new OrderDto
			{
				Id = order.Id,
				ProductId = order.ProductId,
				Quantity = order.Quantity,
				Total = order.Total,
				ClientId = order.ClientId,
				OrderDate = order.OrderDate,
				LoggedInEmployeeId = order.LoggedInEmployeeId
			};
		}

		public async Task UpdateAsync(OrderDto dto)
		{
			if (!dto.Id.HasValue)
				throw new ArgumentException("Order ID is required for update.");

			var existing = await _repository.GetByIdAsync(dto.Id.Value);
			if (existing == null)
				throw new KeyNotFoundException($"Order with ID {dto.Id} not found.");

			// Re-validate product if changed
			if (existing.ProductId != dto.ProductId)
			{
				var response = await _productHttpClient.GetAsync($"api/products/{dto.ProductId}");
				if (!response.IsSuccessStatusCode)
				{
					throw new ArgumentException($"Product with ID {dto.ProductId} not found.");
				}
				var productJson = await response.Content.ReadAsStringAsync();
				var product = JsonSerializer.Deserialize<ProductDto>(productJson, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
				if (product == null)
					throw new ArgumentException("Invalid product data from Product Service.");

				existing = new Order(existing.Id, dto.ProductId, dto.Quantity, dto.Quantity * product.Price, dto.ClientId, dto.OrderDate, existing.LoggedInEmployeeId);
			}
			else
			{
				// Recalculate total
				existing.Update(dto.Quantity, dto.Quantity * existing.Total / existing.Quantity, dto.OrderDate);
			}

			await _repository.UpdateAsync(existing);

			_logger.Information("Order {OrderId} updated by employee {EmployeeId}", existing.Id, existing.LoggedInEmployeeId);
		}

		public async Task DeleteAsync(Guid id)
		{
			await _repository.DeleteAsync(id);
			_logger.Information("Order {OrderId} deleted", id);
		}
	}
	public class ProductDto
	{
		public Guid Id { get; set; }
		public string Name { get; set; } = string.Empty;
		public decimal Price { get; set; }
	}
}