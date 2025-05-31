// AuthService.Infrastructure/Repositories/UserRepository.cs

using System.Threading.Tasks;
using Auth.Domain.Entities;
using Auth.Domain.Interfaces;
using Auth.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Auth.Infrastructure.Repositories
{
	public class UserRepository : IUserRepository
	{
		private readonly AuthDbContext _context;

		public UserRepository(AuthDbContext context)
		{
			_context = context;
		}

		public async Task<User?> GetByUsernameAsync(string username)
		{
			return await _context.Users
								 .FirstOrDefaultAsync(u => u.Username == username);
		}

		public async Task AddAsync(User user)
		{
			await _context.Users.AddAsync(user);
			await _context.SaveChangesAsync();
		}
	}
}
