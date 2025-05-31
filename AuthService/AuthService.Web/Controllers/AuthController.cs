using Auth.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Shared.Application.DTOs; // Assuming DTOs like RegisterRequestDto, LoginRequestDto, AuthResponseDto are here

namespace AuthService.Web.Controllers;

/// <summary>
/// Controller responsible for handling user authentication and registration.
/// </summary>
[ApiController]
[Route("api/[controller]")] // Base route for this controller will be /api/Auth
public class AuthController(IAuthService _authService) : ControllerBase
{
	/// <summary>
	/// Registers a new user in the system.
	/// </summary>
	/// <param name="dto">The data transfer object containing user registration details (e.g., username, email, password).</param>
	/// <returns>
	/// An <see cref="OkResult"/> with a success message if registration is successful.
	/// A <see cref="BadRequestObjectResult"/> if registration fails (e.g., user already exists, invalid data).
	/// </returns>
	[HttpPost("register")] // Route: POST /api/Auth/register
	public async Task<IActionResult> Register([FromBody] RegisterRequestDto dto)
	{
		try
		{
			// Call the authentication service to handle the registration logic
			await _authService.RegisterAsync(dto);
			// Return a 200 OK response with a success message
			return Ok(new { message = "User registered successfully." });
		}
		catch (ApplicationException ex) // Catch specific application exceptions (e.g., validation errors from service layer)
		{
			// Return a 400 Bad Request response with the error message from the exception
			return BadRequest(new { error = ex.Message });
		}
	}

	/// <summary>
	/// Authenticates an existing user and returns a JWT token upon successful authentication.
	/// </summary>
	/// <param name="dto">The data transfer object containing user login credentials (e.g., email, password).</param>
	/// <returns>
	/// An <see cref="OkObjectResult"/> containing the <see cref="AuthResponseDto"/> (with JWT token and user details) if authentication is successful.
	/// An <see cref="UnauthorizedObjectResult"/> if authentication fails (e.g., invalid credentials).
	/// </returns>
	[HttpPost("login")] // Route: POST /api/Auth/login
	public async Task<IActionResult> Login([FromBody] LoginRequestDto dto)
	{
		try
		{
			// Call the authentication service to handle the login logic and token generation
			var response = await _authService.AuthenticateAsync(dto);
			// Return a 200 OK response with the AuthResponseDto (containing the token)
			return Ok(response);
		}
		catch (UnauthorizedAccessException) // Catch exception thrown when credentials are invalid
		{
			// Return a 401 Unauthorized response with an error message
			return Unauthorized(new { error = "Invalid credentials." });
		}
	}
}
