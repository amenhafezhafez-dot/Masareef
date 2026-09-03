using Masareef.BusinessLayer.Services;
using Masareef.Entities.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace Masareef.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly AuthService _service;
    public AuthController(AuthService service) => _service = service;

    /// <summary>تسجيل مستخدم جديد</summary>
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterDto dto)
    {
        var r = await _service.RegisterAsync(dto);
        return r.Success
            ? Ok(r.Data)
            : UnprocessableEntity(new { error = r.Message });
    }

    /// <summary>تسجيل الدخول — بيرجّع التوكن</summary>
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto dto)
    {
        var r = await _service.LoginAsync(dto);
        return r.Success
            ? Ok(r.Data)
            : Unauthorized(new { error = r.Message });
    }
}