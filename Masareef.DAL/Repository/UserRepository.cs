using Masareef.DAL.Context;
using Masareef.Entities.Models;
using Microsoft.EntityFrameworkCore;

namespace Masareef.DAL.Repository;

public class UserRepository : IUserRepository
{
    private readonly AppDbContext _context;

    public UserRepository(AppDbContext context) => _context = context;

    /// <summary>هل فيه مستخدم بالإيميل ده؟ (للتسجيل — نمنع التكرار)</summary>
    public async Task<bool> EmailExistsAsync(string email) =>
        await _context.Users
            .AsNoTracking()
            .AnyAsync(u => u.Email == email.ToLower());

    /// <summary>هات المستخدم بالإيميل (للدخول)</summary>
    public async Task<User?> GetByEmailAsync(string email) =>
        await _context.Users
            .FirstOrDefaultAsync(u => u.Email == email.ToLower());

    /// <summary>هات المستخدم بالرقم</summary>
    public async Task<User?> GetByIdAsync(int userId) =>
        await _context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.UserId == userId);

    /// <summary>ضيف مستخدم جديد وارجّع رقمه</summary>
    public async Task<int> AddUserAsync(User user)
    {
        _context.Users.Add(user);
        await _context.SaveChangesAsync();
        return user.UserId;
    }
}