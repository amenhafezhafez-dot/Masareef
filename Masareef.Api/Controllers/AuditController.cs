using Masareef.BusinessLayer.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Masareef.Api.Controllers;

/// <summary>سجل التدقيق — الـ Admin بس اللي يشوفه</summary>
[Route("api/admin/audit")]
[Authorize(Policy = "AdminOnly")]
public class AuditController : ApiControllerBase
{
    private readonly AuditService _service;
    public AuditController(AuditService service) => _service = service;

    /// <summary>آخر 200 حدث — مع فلترة اختيارية باليوزر أو نوع الحدث</summary>
    [HttpGet]
    public async Task<IActionResult> Get(
        [FromQuery] int? userId, [FromQuery] string? action) =>
        FromResult(await _service.SearchAsync(userId, action));
}
