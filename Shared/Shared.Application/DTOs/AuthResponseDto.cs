namespace Shared.Application.DTOs;

public class AuthResponseDto
{
	public string AccessToken { get; set; } = string.Empty;
	public int ExpiresIn { get; set; }
}
