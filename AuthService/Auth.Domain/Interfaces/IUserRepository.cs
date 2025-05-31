using Auth.Domain.Entities;

namespace Auth.Domain.Interfaces
{
	public interface IUserRepository
	{
		Task<User?> GetByUsernameAsync(string username);
		Task AddAsync(User user);
	}
}
