using Login.Business.Interfaces;
using Shared.Application.DTOs;
using Shared.Infrastructure.Clients;
using System.Net.Http.Json;

namespace Login.Business.Services;

public class LoginService(DefaultHttpClient _httpClient) : ILoginService
{
    public async Task<AuthResponseDto> LoginUser(string username, string password)
    {
        var requestBody = new LoginRequestDto
        {
            Username = username,
            Password = password
        };
        var response = await _httpClient.PostAsJsonAsync("/api/auth/login", requestBody);

        if (response.IsSuccessStatusCode)
        {
            return await response.Content.ReadFromJsonAsync<AuthResponseDto>();
        }

        if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            throw new UnauthorizedAccessException();

        var errorMessage = await response.Content.ReadAsStringAsync();
        throw new Exception(errorMessage);
    }

    public async Task<AuthResponseDto> RegisterUser(string username, string password)
    {
        var requestBody = new LoginRequestDto
        {
            Username = username,
            Password = password
        };
        var response = await _httpClient.PostAsJsonAsync("/api/auth/register", requestBody);

        if (response.IsSuccessStatusCode)
        {
            return await response.Content.ReadFromJsonAsync<AuthResponseDto>();
        }

        var errorMessage = await response.Content.ReadAsStringAsync();
        throw new Exception(errorMessage);
    }
}
