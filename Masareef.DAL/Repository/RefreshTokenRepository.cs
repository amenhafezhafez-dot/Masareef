using Masareef.DAL.Context;
using Masareef.Entities.Models;
using Microsoft.EntityFrameworkCore;

namespace Masareef.DAL.Repository;

public class RefreshTokenRepository : IRefreshTokenRepository
{
    private readonly AppDbContext _context;

    public RefreshTokenRepository(AppDbContext context) => _context = context;

    public async Task AddAsync(RefreshToken token)
    {
        _context.RefreshTokens.Add(token);
        await _context.SaveChangesAsync();
    }

    /// <summary>بيرجع التوكن سواء كان فعّال أو منتهي/ملغي — التحقق من الصلاحية مسؤولية الـ Service</summary>
    public async Task<RefreshToken?> GetByHashAsync(string tokenHash) =>
        await _context.RefreshTokens
            .FirstOrDefaultAsync(t => t.TokenHash == tokenHash);

    public async Task RevokeAsync(RefreshToken token, string? replacedByTokenHash = null)
    {
        token.RevokedAt = DateTime.UtcNow;
        token.ReplacedByTokenHash = replacedByTokenHash;
        await _context.SaveChangesAsync();
    }
}
