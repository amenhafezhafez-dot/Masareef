using Masareef.Entities.Models;

namespace Masareef.DAL.Repository;

public interface IRefreshTokenRepository
{
    Task AddAsync(RefreshToken token);
    Task<RefreshToken?> GetByHashAsync(string tokenHash);
    Task RevokeAsync(RefreshToken token, string? replacedByTokenHash = null);
}
