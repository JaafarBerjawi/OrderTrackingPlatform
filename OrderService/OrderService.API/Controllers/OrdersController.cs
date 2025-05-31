using Microsoft.AspNetCore.Mvc;
using Shared.Application.DTOs;
using OrderService.Application.Interfaces;

namespace OrderService.API.Controllers
{
	[ApiController]
	[Route("api/[controller]")]
	public class OrdersController : ControllerBase
	{
		private readonly IOrderService _service;

		public OrdersController(IOrderService service)
		{
			_service = service;
		}

		[HttpGet]
		public async Task<IActionResult> GetAllByEmployee()
		{
			// Simulated extraction of employee ID from header
			if (!Request.Headers.TryGetValue("X-Employee-Id", out var empHeader) || !Guid.TryParse(empHeader, out var employeeId))
			{
				return BadRequest("Missing or invalid X-Employee-Id header.");
			}

			var orders = await _service.GetAllByEmployeeAsync(employeeId);
			return Ok(orders);
		}

		[HttpGet("{id}")]
		public async Task<IActionResult> GetById(Guid id)
		{
			var order = await _service.GetByIdAsync(id);
			if (order == null) return NotFound();
			return Ok(order);
		}

		[HttpPost]
		public async Task<IActionResult> Create([FromBody] OrderDto dto)
		{
			if (!Request.Headers.TryGetValue("X-Employee-Id", out var empHeader) || !Guid.TryParse(empHeader, out var employeeId))
			{
				return BadRequest("Missing or invalid X-Employee-Id header.");
			}

			dto.LoggedInEmployeeId = employeeId;
			var created = await _service.CreateAsync(dto);
			return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
		}

		[HttpPut("{id}")]
		public async Task<IActionResult> Update(Guid id, [FromBody] OrderDto dto)
		{
			if (!dto.Id.HasValue || id != dto.Id) return BadRequest("ID mismatch.");

			await _service.UpdateAsync(dto);
			return NoContent();
		}

		[HttpDelete("{id}")]
		public async Task<IActionResult> Delete(Guid id)
		{
			await _service.DeleteAsync(id);
			return NoContent();
		}
	}
}