using Shared.Application.DTOs;

namespace Login.Business.Interfaces
{
    public interface ILoginService
    {
        Task<AuthResponseDto> LoginUser(string username, string password);
        Task<AuthResponseDto> RegisterUser(string username, string password);
    }
}
