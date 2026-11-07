using Masareef.DAL.Repository;
using Masareef.Entities.Common;
using Masareef.Entities.DTOs;
using Masareef.Entities.Models;

namespace Masareef.BusinessLayer.Services;

/// <summary>
/// بيلف الـ AuthResultDto (اللي بيترجع في الـ response body) مع الـ Refresh Token الخام —
/// ده الوحيد اللي مسموح يشوفه هو الـ Controller عشان يحطه في الـ HttpOnly cookie،
/// ومبيتخزنش ولا بيترجع أبداً كجزء من الـ JSON body.
/// </summary>
public record AuthTokens(AuthResultDto Result, string RefreshToken);

public class AuthService
{
    private readonly IUserRepository _repo;
    private readonly IRefreshTokenRepository _refreshTokens;
    private readonly TokenService _tokens;

    public AuthService(IUserRepository repo, IRefreshTokenRepository refreshTokens, TokenService tokens)
    {
        _repo = repo;
        _refreshTokens = refreshTokens;
        _tokens = tokens;
    }


    // ═══════════════════ REGISTER ═══════════════════

    public async Task<Result<AuthTokens>> RegisterAsync(RegisterDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Name))
            return Result<AuthTokens>.Fail("Name is required.");

        if (string.IsNullOrWhiteSpace(dto.Email))
            return Result<AuthTokens>.Fail("Email is required.");

        if (string.IsNullOrWhiteSpace(dto.Password) || dto.Password.Length < 6)
            return Result<AuthTokens>.Fail("Password must be at least 6 characters.");

        // الإيميل متسجّل قبل كده؟
        if (await _repo.EmailExistsAsync(dto.Email.Trim()))
            return Result<AuthTokens>.Fail("This email is already registered.");

        // تخزين آمن للباسورد — BCrypt بيعمل hash + salt تلقائياً
        string hash = BCrypt.Net.BCrypt.HashPassword(dto.Password);

        var user = new User
        {
            Name = dto.Name.Trim(),
            Email = dto.Email.Trim().ToLowerInvariant(),
            PasswordHash = hash,
            PhoneNumber = dto.PhoneNumber?.Trim(),
            Role = Roles.User,               // 🔒 التسجيل العادي دايماً User — مفيش Admin من الـ API
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        int newId = await _repo.AddUserAsync(user);
        user.UserId = newId;

        // نولّد توكنات على طول عشان المستخدم يدخل بعد التسجيل مباشرة
        return Result<AuthTokens>.Ok(await BuildTokensAsync(user));
    }


    // ═══════════════════ LOGIN ═══════════════════

    public async Task<Result<AuthTokens>> LoginAsync(LoginDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Email) || string.IsNullOrWhiteSpace(dto.Password))
            return Result<AuthTokens>.Fail("Email and password are required.");

        var user = await _repo.GetByEmailAsync(dto.Email.Trim().ToLowerInvariant());

        // نفس الرسالة سواء الإيميل غلط أو الباسورد غلط
        if (user is null || !BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
            return Result<AuthTokens>.Fail("Invalid email or password.");

        if (!user.IsActive)
            return Result<AuthTokens>.Fail("This account is disabled.");

        return Result<AuthTokens>.Ok(await BuildTokensAsync(user));
    }


    // ═══════════════════ REFRESH ═══════════════════

    public async Task<Result<AuthTokens>> RefreshAsync(string? rawRefreshToken)
    {
        if (string.IsNullOrWhiteSpace(rawRefreshToken))
            return Result<AuthTokens>.Fail("Refresh token is required.");

        var hash = _tokens.HashRefreshToken(rawRefreshToken);
        var stored = await _refreshTokens.GetByHashAsync(hash);

        if (stored is null || !stored.IsActive)
            return Result<AuthTokens>.Fail("Invalid or expired refresh token.");

        var user = await _repo.GetByIdAsync(stored.UserId);
        if (user is null || !user.IsActive)
            return Result<AuthTokens>.Fail("Account not available.");

        // Rotation: التوكن القديم بيتلغي فورًا وميتستخدمش تاني، حتى لو لسه معاه وقت
        var tokens = await BuildTokensAsync(user);
        await _refreshTokens.RevokeAsync(stored, _tokens.HashRefreshToken(tokens.RefreshToken));

        return Result<AuthTokens>.Ok(tokens);
    }


    // ═══════════════════ LOGOUT ═══════════════════

    public async Task LogoutAsync(string? rawRefreshToken)
    {
        if (string.IsNullOrWhiteSpace(rawRefreshToken))
            return;

        var hash = _tokens.HashRefreshToken(rawRefreshToken);
        var stored = await _refreshTokens.GetByHashAsync(hash);

        if (stored is not null && stored.RevokedAt is null)
            await _refreshTokens.RevokeAsync(stored);
    }


    // ---------- مساعد ----------
    private async Task<AuthTokens> BuildTokensAsync(User user)
    {
        var (accessToken, expiresAt) = _tokens.Create(user);
        var rawRefreshToken = _tokens.GenerateRefreshToken();

        await _refreshTokens.AddAsync(new RefreshToken
        {
            UserId = user.UserId,
            TokenHash = _tokens.HashRefreshToken(rawRefreshToken),
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddDays(_tokens.RefreshTokenExpiryDays)
        });

        var result = new AuthResultDto
        {
            Token = accessToken,
            UserId = user.UserId,
            Name = user.Name,
            Email = user.Email,
            Role = user.Role,
            ExpiresAt = expiresAt
        };

        return new AuthTokens(result, rawRefreshToken);
    }
}
