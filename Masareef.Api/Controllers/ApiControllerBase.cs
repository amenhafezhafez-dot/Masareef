using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;        // ← جديد
using System.Security.Claims;                     // ← جديد
using Masareef.Entities.Common;

namespace Masareef.Api.Controllers;

[ApiController]
[Authorize]                                       // ← جديد — كل الكونترولرز محتاجة توكن
public abstract class ApiControllerBase : ControllerBase
{
    /// <summary>الـ UserId من التوكن بدل الرقم الثابت</summary>
    protected int CurrentUserId =>
        int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    protected IActionResult FromResult<T>(Result<T> r) =>
        r.Success ? Ok(r.Data) : Fail(r.Message!);

    protected IActionResult FromResult(Result r) =>
        r.Success ? NoContent() : Fail(r.Message!);

    private IActionResult Fail(string message) =>
        message.Contains("not found", StringComparison.OrdinalIgnoreCase)
            ? NotFound(new { error = message })
            : UnprocessableEntity(new { error = message });
}