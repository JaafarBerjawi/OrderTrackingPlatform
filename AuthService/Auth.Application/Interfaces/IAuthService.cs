using System.Threading.Tasks;
using Shared.Application.DTOs;

namespace Auth.Application.Interfaces;

public interface IAuthService
{
	Task RegisterAsync(RegisterRequestDto dto);
	Task<AuthResponseDto> AuthenticateAsync(LoginRequestDto dto);
}

