using Masareef.BusinessLayer.Services;
using Masareef.Entities.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace Masareef.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private const string RefreshCookieName = "refreshToken";

    private readonly AuthService _service;
    public AuthController(AuthService service) => _service = service;

    /// <summary>تسجيل مستخدم جديد</summary>
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterDto dto)
    {
        var r = await _service.RegisterAsync(dto);
        if (!r.Success)
            return UnprocessableEntity(new { error = r.Message });

        SetRefreshTokenCookie(r.Data!.RefreshToken);
        return Ok(r.Data!.Result);
    }

    /// <summary>تسجيل الدخول — بيرجّع الـ access token، والـ refresh token بيتحط في HttpOnly cookie</summary>
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto dto)
    {
        var r = await _service.LoginAsync(dto);
        if (!r.Success)
            return Unauthorized(new { error = r.Message });

        SetRefreshTokenCookie(r.Data!.RefreshToken);
        return Ok(r.Data!.Result);
    }

    /// <summary>بيقرا الـ refresh token من الكوكي ويرجّع access token جديد (مع تدوير الـ refresh token)</summary>
    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh()
    {
        var raw = Request.Cookies[RefreshCookieName];
        var r = await _service.RefreshAsync(raw);

        if (!r.Success)
        {
            DeleteRefreshTokenCookie();
            return Unauthorized(new { error = r.Message });
        }

        SetRefreshTokenCookie(r.Data!.RefreshToken);
        return Ok(r.Data!.Result);
    }

    /// <summary>بيلغي الـ refresh token الحالي ويمسح الكوكي</summary>
    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        var raw = Request.Cookies[RefreshCookieName];
        await _service.LogoutAsync(raw);

        DeleteRefreshTokenCookie();
        return NoContent();
    }


    // ---------- مساعد ----------

    private void SetRefreshTokenCookie(string rawRefreshToken)
    {
        Response.Cookies.Append(RefreshCookieName, rawRefreshToken, new CookieOptions
        {
            HttpOnly = true,                              // JavaScript منعه يقراها خالص
            Secure = true,                                 // يترسل بس فوق HTTPS
            SameSite = SameSiteMode.None,                  // الفرونت إند على origin مختلف (بورت مختلف)
            Path = "/api/auth",                            // تترسل بس لـ endpoints الـ auth، مش لكل الـ API
            Expires = DateTimeOffset.UtcNow.AddDays(7)      // لازم يطابق Jwt:RefreshTokenExpiryDays
        });
    }

    private void DeleteRefreshTokenCookie() =>
        Response.Cookies.Delete(RefreshCookieName, new CookieOptions { Path = "/api/auth" });
}
