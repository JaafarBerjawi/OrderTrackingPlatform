using Auth.Application.Interfaces;
using Shared.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Auth.Domain.Entities;
using Auth.Domain.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;

namespace Auth.Application.Services
{
	public class AuthService : IAuthService
	{
		private readonly IUserRepository _userRepository;
		private readonly IConfiguration _configuration;

		public AuthService(
			IUserRepository userRepository,
			IConfiguration configuration)
		{
			_userRepository = userRepository;
			_configuration = configuration;
		}

		public async Task RegisterAsync(RegisterRequestDto dto)
		{
			// 1) Check if username or email already exists
			if (await _userRepository.GetByUsernameAsync(dto.Username) != null)
				throw new ApplicationException("Username is already taken.");

			// 2) Hash the password
			string hashedPassword = BCrypt.Net.BCrypt.HashPassword(dto.Password);

			// 3) Create and save the user entity
			var user = new User
			{
				Id = Guid.NewGuid(),
				Username = dto.Username,
				PasswordHash = hashedPassword
				// Optionally: assign a default role/claim here
				// e.g. Role = dto.Role
			};

			await _userRepository.AddAsync(user);
		}

		public async Task<AuthResponseDto> AuthenticateAsync(LoginRequestDto dto)
		{
			// 1) Look up the user by username or email
			User? user = await _userRepository.GetByUsernameAsync(dto.Username);

			if (user == null)
				throw new UnauthorizedAccessException("Invalid credentials.");

			// 2) Verify password
			bool isPasswordValid = BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash);
			if (!isPasswordValid)
				throw new UnauthorizedAccessException("Invalid credentials.");

			// 3) Generate a JWT for this user
			var jwtSettings = _configuration.GetSection("JwtSettings");
			var secretKey = jwtSettings["SecretKey"] ?? throw new Exception("JwtSettings:SecretKey is missing");
			var issuer = jwtSettings["Issuer"] ?? throw new Exception("JwtSettings:Issuer is missing");
			var audience = jwtSettings["Audience"] ?? throw new Exception("JwtSettings:Audience is missing");
			int expiryMins = int.Parse(jwtSettings["AccessTokenExpiryMinutes"] ?? "15");

			// Build claims 
			var claims = new List<Claim>
			{
				new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
				new Claim(JwtRegisteredClaimNames.UniqueName, user.Username),
                // Optionally add roles or other claims:
                // new Claim(ClaimTypes.Role, user.Role)
            };

			var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
			var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

			var tokenDescriptor = new JwtSecurityToken(
				issuer: issuer,
				audience: audience,
				claims: claims,
				notBefore: DateTime.UtcNow,
				expires: DateTime.UtcNow.AddMinutes(expiryMins),
				signingCredentials: creds
			);

			string accessToken = new JwtSecurityTokenHandler().WriteToken(tokenDescriptor);

			return new AuthResponseDto
			{
				AccessToken = accessToken,
				ExpiresIn = expiryMins * 60  // convert minutes → seconds
			};
		}
	}
}
