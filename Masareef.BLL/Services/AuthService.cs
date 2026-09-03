using Masareef.DAL.Repository;
using Masareef.Entities.Common;
using Masareef.Entities.DTOs;
using Masareef.Entities.Models;

namespace Masareef.BusinessLayer.Services;

public class AuthService
{
    private readonly IUserRepository _repo;
    private readonly TokenService _tokens;

    public AuthService(IUserRepository repo, TokenService tokens)
    {
        _repo = repo;
        _tokens = tokens;
    }


    // ═══════════════════ REGISTER ═══════════════════

    public async Task<Result<AuthResultDto>> RegisterAsync(RegisterDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Name))
            return Result<AuthResultDto>.Fail("Name is required.");

        if (string.IsNullOrWhiteSpace(dto.Email))
            return Result<AuthResultDto>.Fail("Email is required.");

        if (string.IsNullOrWhiteSpace(dto.Password) || dto.Password.Length < 6)
            return Result<AuthResultDto>.Fail("Password must be at least 6 characters.");

        // الإيميل متسجّل قبل كده؟
        if (await _repo.EmailExistsAsync(dto.Email.Trim()))
            return Result<AuthResultDto>.Fail("This email is already registered.");

        // تخزين آمن للباسورد — BCrypt بيعمل hash + salt تلقائياً
        string hash = BCrypt.Net.BCrypt.HashPassword(dto.Password);

        var user = new User
        {
            Name = dto.Name.Trim(),
            Email = dto.Email.Trim().ToLowerInvariant(),
            PasswordHash = hash,
            PhoneNumber = dto.PhoneNumber?.Trim(),
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        int newId = await _repo.AddUserAsync(user);
        user.UserId = newId;

        // نولّد توكن على طول عشان المستخدم يدخل بعد التسجيل مباشرة
        return Result<AuthResultDto>.Ok(BuildResult(user));
    }


    // ═══════════════════ LOGIN ═══════════════════

    public async Task<Result<AuthResultDto>> LoginAsync(LoginDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Email) || string.IsNullOrWhiteSpace(dto.Password))
            return Result<AuthResultDto>.Fail("Email and password are required.");

        var user = await _repo.GetByEmailAsync(dto.Email.Trim().ToLowerInvariant());

        // نفس الرسالة سواء الإيميل غلط أو الباسورد غلط
        if (user is null || !BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
            return Result<AuthResultDto>.Fail("Invalid email or password.");

        if (!user.IsActive)
            return Result<AuthResultDto>.Fail("This account is disabled.");

        return Result<AuthResultDto>.Ok(BuildResult(user));
    }


    // ---------- مساعد ----------
    private AuthResultDto BuildResult(User user)
    {
        var (token, expiresAt) = _tokens.Create(user);

        return new AuthResultDto
        {
            Token = token,
            UserId = user.UserId,
            Name = user.Name,
            Email = user.Email,
            ExpiresAt = expiresAt
        };
    }
}