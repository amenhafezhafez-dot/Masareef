using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Masareef.Entities.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace Masareef.BusinessLayer.Services;

public class TokenService
{
    private readonly IConfiguration _config;

    public TokenService(IConfiguration config) => _config = config;

    /// <summary>بيولّد توكن موقّع فيه هوية المستخدم</summary>
    public (string token, DateTime expiresAt) Create(User user)
    {
        var jwt = _config.GetSection("Jwt");

        // المفتاح السري — لازم يطابق اللي في appsettings.json
        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(jwt["Key"]!));

        var credentials = new SigningCredentials(
            key, SecurityAlgorithms.HmacSha256);

        // الـ claims = المعلومات اللي جوه التوكن
        var claims = new[]
        {
            // الأهم: الـ UserId — هنقراه بعدين بدل CurrentUserId الثابت
            new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim("name", user.Name),
            new Claim(ClaimTypes.Role, user.Role),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        int minutes = int.TryParse(jwt["ExpiryMinutes"], out var m) ? m : 30;
        var expiresAt = DateTime.UtcNow.AddMinutes(minutes);

        var token = new JwtSecurityToken(
            issuer: jwt["Issuer"],
            audience: jwt["Audience"],
            claims: claims,
            expires: expiresAt,
            signingCredentials: credentials);

        string tokenString = new JwtSecurityTokenHandler().WriteToken(token);

        return (tokenString, expiresAt);
    }


    /// <summary>بيولّد Refresh Token عشوائي وآمن (256-bit) — ده اللي بيتبعت للكلاينت في الكوكي</summary>
    public string GenerateRefreshToken() =>
        Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));

    /// <summary>بنخزن الـ hash بس في الداتابيز، مش التوكن الخام — لو الداتابيز اتسرقت التوكنات تفضل غير قابلة للاستخدام</summary>
    public string HashRefreshToken(string rawToken) =>
        Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(rawToken)));

    public int RefreshTokenExpiryDays =>
        int.TryParse(_config.GetSection("Jwt")["RefreshTokenExpiryDays"], out var d) ? d : 7;
}