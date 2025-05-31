using Microsoft.AspNetCore.Mvc;
using Shared.Application.DTOs; // Contains OrderDto for request/response
using OrderService.Application.Interfaces; // Contains IOrderService

namespace OrderService.API.Controllers
{
	/// <summary>
	/// API controller for managing customer orders.
	/// Relies on an 'X-Employee-Id' header, assumed to be injected by the API Gateway,
	/// to identify the user associated with order operations.
	/// </summary>
	[ApiController]
	[Route("api/[controller]")] // Base route: /api/Orders
	public class OrdersController : ControllerBase
	{
		private readonly IOrderService _service;

		/// <summary>
		/// Initializes a new instance of the <see cref="OrdersController"/> class.
		/// </summary>
		/// <param name="service">The order service instance for business logic.</param>
		public OrdersController(IOrderService service)
		{
			_service = service;
		}

		/// <summary>
		/// Retrieves all orders associated with the employee ID passed in the 'X-Employee-Id' header.
		/// </summary>
		/// <returns>
		/// An <see cref="OkObjectResult"/> containing a list of <see cref="OrderDto"/> if successful.
		/// A <see cref="BadRequestObjectResult"/> if the 'X-Employee-Id' header is missing or invalid.
		/// </returns>
		[HttpGet] // Route: GET /api/Orders
		public async Task<IActionResult> GetAllByEmployee()
		{
			// Extract employee ID from the "X-Employee-Id" header.
			// This header is expected to be set by the API Gateway after authenticating the user.
			if (!Request.Headers.TryGetValue("X-Employee-Id", out var empHeaderValue) ||
			    !Guid.TryParse(empHeaderValue.FirstOrDefault(), out var employeeId))
			{
				return BadRequest(new { message = "Missing or invalid X-Employee-Id header." });
			}

			// Call the service to get all orders for the given employee ID
			var orders = await _service.GetAllByEmployeeAsync(employeeId);
			// Return 200 OK with the list of orders
			return Ok(orders);
		}

		/// <summary>
		/// Retrieves a specific order by its ID.
		/// It's implied that authorization (e.g., if the order belongs to the requesting user)
		/// might be handled within the service layer or by convention that users can only fetch their own orders via GetOrderById.
		/// </summary>
		/// <param name="id">The GUID ID of the order to retrieve.</param>
		/// <returns>
		/// An <see cref="OkObjectResult"/> containing the <see cref="OrderDto"/> if found.
		/// A <see cref="NotFoundResult"/> if the order with the given ID does not exist.
		/// </returns>
		[HttpGet("{id}")] // Route: GET /api/Orders/{id}
		public async Task<IActionResult> GetById(Guid id)
		{
			// Call the service to get the order DTO by ID
			var order = await _service.GetByIdAsync(id);
			// If order is not found, return 404 Not Found
			if (order == null) return NotFound();
			// Otherwise, return 200 OK with the order DTO
			return Ok(order);
		}

		/// <summary>
		/// Creates a new order. The employee ID for the order is taken from the 'X-Employee-Id' header.
		/// </summary>
		/// <param name="dto">The <see cref="OrderDto"/> containing the data for the new order.
		/// The `LoggedInEmployeeId` property of the DTO will be populated from the header.
		/// The ID in the DTO is typically ignored or should be empty/Guid.Empty for creation.</param>
		/// <returns>
		/// A <see cref="CreatedAtActionResult"/> (201 Created) with the newly created <see cref="OrderDto"/> and a location header.
		/// A <see cref="BadRequestObjectResult"/> if the 'X-Employee-Id' header is missing or invalid.
		/// </returns>
		[HttpPost] // Route: POST /api/Orders
		public async Task<IActionResult> Create([FromBody] OrderDto dto)
		{
			// Extract employee ID from the "X-Employee-Id" header
			if (!Request.Headers.TryGetValue("X-Employee-Id", out var empHeaderValue) ||
			    !Guid.TryParse(empHeaderValue.FirstOrDefault(), out var employeeId))
			{
				return BadRequest(new { message = "Missing or invalid X-Employee-Id header." });
			}

			// Assign the extracted employee ID to the DTO before passing to the service
			dto.LoggedInEmployeeId = employeeId;

			// Call the service to create a new order
			var createdOrder = await _service.CreateAsync(dto);
			// Return 201 Created with:
			// - The action name to get the created resource ("GetById")
			// - The route values for the GetById action (the ID of the new order)
			// - The created order DTO itself
			return CreatedAtAction(nameof(GetById), new { id = createdOrder.Id }, createdOrder);
		}

		/// <summary>
		/// Updates an existing order.
		/// Note: The current implementation in the provided snippet only updates based on the DTO.
		/// It does not explicitly check if the 'X-Employee-Id' header matches the owner of the order.
		/// Such checks might be part of the service layer's responsibility.
		/// </summary>
		/// <param name="id">The GUID ID of the order to update. This must match the ID in the request body DTO.</param>
		/// <param name="dto">The <see cref="OrderDto"/> containing the updated data for the order.</param>
		/// <returns>
		/// A <see cref="NoContentResult"/> (204 No Content) if the update is successful.
		/// A <see cref="BadRequestObjectResult"/> if the ID in the URL does not match the ID in the DTO.
		/// </returns>
		[HttpPut("{id}")] // Route: PUT /api/Orders/{id}
		public async Task<IActionResult> Update(Guid id, [FromBody] OrderDto dto)
		{
			// Basic validation: Ensure the ID in the route matches the ID in the request body
			if (!dto.Id.HasValue || id != dto.Id.Value)
			{
				return BadRequest(new { message = "ID mismatch between URL and request body, or ID missing in body." });
			}

			// Potentially extract X-Employee-Id here if service layer needs it for validation,
			// though the current _service.UpdateAsync(dto) signature doesn't show it.
			// For example:
			// if (!Request.Headers.TryGetValue("X-Employee-Id", out var empHeaderValue) ||
			//     !Guid.TryParse(empHeaderValue.FirstOrDefault(), out var employeeId))
			// {
			//     return BadRequest(new { message = "Missing or invalid X-Employee-Id header for update validation." });
			// }
			// dto.LoggedInEmployeeId = employeeId; // If service needs it

			// Call the service to update the order
			await _service.UpdateAsync(dto);
			// Return 204 No Content to indicate successful update
			return NoContent();
		}

		/// <summary>
		/// Deletes an order by its ID.
		/// Note: Similar to Update, explicit check against 'X-Employee-Id' for ownership
		/// is not shown here but might be handled in the service layer.
		/// </summary>
		/// <param name="id">The GUID ID of the order to delete.</param>
		/// <returns>A <see cref="NoContentResult"/> (204 No Content) if the deletion is successful.</returns>
		[HttpDelete("{id}")] // Route: DELETE /api/Orders/{id}
		public async Task<IActionResult> Delete(Guid id)
		{
			// Call the service to delete the order by its ID
			await _service.DeleteAsync(id);
			// Return 204 No Content to indicate successful deletion
			return NoContent();
		}
	}
}