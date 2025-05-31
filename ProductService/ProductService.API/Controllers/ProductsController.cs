using Microsoft.AspNetCore.Mvc;
using Shared.Application.DTOs;
using ProductService.Application.Interfaces;

namespace ProductService.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductsController(IProductService _service) : ControllerBase
{
	[HttpGet]
	public async Task<IActionResult> GetAll()
	{
		var products = await _service.GetAllAsync();
		return Ok(products);
	}

	[HttpGet("{id}")]
	public async Task<IActionResult> GetById(Guid id)
	{
		var product = await _service.GetByIdAsync(id);
		if (product == null) return NotFound();
		return Ok(product);
	}

	[HttpPost]
	public async Task<IActionResult> Create([FromBody] ProductDto dto)
	{
		var created = await _service.CreateAsync(dto);
		return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
	}

	[HttpPut("{id}")]
	public async Task<IActionResult> Update(Guid id, [FromBody] ProductDto dto)
	{
		if (id != dto.Id) return BadRequest("ID mismatch.");
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