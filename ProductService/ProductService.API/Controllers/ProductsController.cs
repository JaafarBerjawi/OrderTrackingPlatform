using Microsoft.AspNetCore.Mvc;
using Shared.Application.DTOs; // Contains ProductDto for request/response
using ProductService.Application.Interfaces; // Contains IProductService

namespace ProductService.API.Controllers;

/// <summary>
/// API controller for managing products.
/// Provides CRUD operations for products.
/// </summary>
[ApiController]
[Route("api/[controller]")] // Base route: /api/Products
public class ProductsController(IProductService _service) : ControllerBase
{
	/// <summary>
	/// Retrieves all products.
	/// </summary>
	/// <returns>An <see cref="OkObjectResult"/> containing a list of <see cref="ProductDto"/>.</returns>
	[HttpGet] // Route: GET /api/Products
	public async Task<IActionResult> GetAll()
	{
		// Call the service to get all product DTOs
		var products = await _service.GetAllAsync();
		// Return 200 OK with the list of products
		return Ok(products);
	}

	/// <summary>
	/// Retrieves a specific product by its ID.
	/// </summary>
	/// <param name="id">The GUID ID of the product to retrieve.</param>
	/// <returns>
	/// An <see cref="OkObjectResult"/> containing the <see cref="ProductDto"/> if found.
	/// A <see cref="NotFoundResult"/> if the product with the given ID does not exist.
	/// </returns>
	[HttpGet("{id}")] // Route: GET /api/Products/{id}
	public async Task<IActionResult> GetById(Guid id)
	{
		// Call the service to get the product DTO by ID
		var product = await _service.GetByIdAsync(id);
		// If product is not found, return 404 Not Found
		if (product == null) return NotFound();
		// Otherwise, return 200 OK with the product DTO
		return Ok(product);
	}

	/// <summary>
	/// Creates a new product.
	/// </summary>
	/// <param name="dto">The <see cref="ProductDto"/> containing the data for the new product.
	/// The ID in the DTO is typically ignored or should be empty/Guid.Empty for creation.</param>
	/// <returns>
	/// A <see cref="CreatedAtActionResult"/> (201 Created) with the newly created <see cref="ProductDto"/> and a location header pointing to the new resource.
	/// </returns>
	[HttpPost] // Route: POST /api/Products
	public async Task<IActionResult> Create([FromBody] ProductDto dto)
	{
		// Call the service to create a new product based on the DTO
		var createdProduct = await _service.CreateAsync(dto);
		// Return 201 Created with:
		// - The action name to get the created resource ("GetById")
		// - The route values for the GetById action (the ID of the new product)
		// - The created product DTO itself
		return CreatedAtAction(nameof(GetById), new { id = createdProduct.Id }, createdProduct);
	}

	/// <summary>
	/// Updates an existing product.
	/// </summary>
	/// <param name="id">The GUID ID of the product to update. This must match the ID in the request body DTO.</param>
	/// <param name="dto">The <see cref="ProductDto"/> containing the updated data for the product.</param>
	/// <returns>
	/// A <see cref="NoContentResult"/> (204 No Content) if the update is successful.
	/// A <see cref="BadRequestObjectResult"/> if the ID in the URL does not match the ID in the DTO.
	/// A <see cref="NotFoundResult"/> if the product to update is not found (handled by the service layer, typically resulting in an exception or specific return code).
	/// </returns>
	[HttpPut("{id}")] // Route: PUT /api/Products/{id}
	public async Task<IActionResult> Update(Guid id, [FromBody] ProductDto dto)
	{
		// Basic validation: Ensure the ID in the route matches the ID in the request body
		if (id != dto.Id) return BadRequest(new { message = "ID mismatch between URL and request body." });

		// Call the service to update the product
		// The service layer should handle logic for not found items, potentially throwing an exception
		// which would then be handled by global exception handling middleware (if configured) or result in a 500 error.
		await _service.UpdateAsync(dto);
		// Return 204 No Content to indicate successful update without returning the entity body
		return NoContent();
	}

	/// <summary>
	/// Deletes a product by its ID.
	/// </summary>
	/// <param name="id">The GUID ID of the product to delete.</param>
	/// <returns>
	/// A <see cref="NoContentResult"/> (204 No Content) if the deletion is successful.
	/// A <see cref="NotFoundResult"/> if the product to delete is not found (handled by the service layer).
	/// </returns>
	[HttpDelete("{id}")] // Route: DELETE /api/Products/{id}
	public async Task<IActionResult> Delete(Guid id)
	{
		// Call the service to delete the product by its ID
		// The service layer should handle logic for not found items.
		await _service.DeleteAsync(id);
		// Return 204 No Content to indicate successful deletion
		return NoContent();
	}
}