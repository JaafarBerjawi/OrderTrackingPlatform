using Auth.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Shared.Application.DTOs;

namespace AuthService.Web.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController(IAuthService _authService) : ControllerBase
{
	[HttpPost("register")]
	public async Task<IActionResult> Register([FromBody] RegisterRequestDto dto)
	{
		try
		{
			await _authService.RegisterAsync(dto);
			return Ok(new { message = "User registered successfully." });
		}
		catch (ApplicationException ex)
		{
			return BadRequest(new { error = ex.Message });
		}
	}

	[HttpPost("login")]
	public async Task<IActionResult> Login([FromBody] LoginRequestDto dto)
	{
		try
		{
			var response = await _authService.AuthenticateAsync(dto);
			return Ok(response);
		}
		catch (UnauthorizedAccessException)
		{
			return Unauthorized(new { error = "Invalid credentials." });
		}
	}
}
