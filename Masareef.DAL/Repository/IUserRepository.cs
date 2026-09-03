using Masareef.Entities.Models;

namespace Masareef.DAL.Repository;

public interface IUserRepository
{
    Task<bool> EmailExistsAsync(string email);
    Task<User?> GetByEmailAsync(string email);
    Task<User?> GetByIdAsync(int userId);
    Task<int> AddUserAsync(User user);
}