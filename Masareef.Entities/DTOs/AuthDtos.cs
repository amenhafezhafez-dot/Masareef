namespace Masareef.Entities.DTOs;

/// <summary>Input — تسجيل مستخدم جديد</summary>
public class RegisterDto
{
    public string Name { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string Password { get; set; } = null!;
    public string? PhoneNumber { get; set; }
}

/// <summary>Input — تسجيل الدخول</summary>
public class LoginDto
{
    public string Email { get; set; } = null!;
    public string Password { get; set; } = null!;
}

/// <summary>Output — اللي بيرجع بعد نجاح الدخول/التسجيل</summary>
public class AuthResultDto
{
    public string Token { get; set; } = null!;
    public int UserId { get; set; }
    public string Name { get; set; } = null!;
    public string Email { get; set; } = null!;
    public DateTime ExpiresAt { get; set; }
}