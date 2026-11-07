using Masareef.DAL.Repository;
using Masareef.Entities.Common;
using Masareef.Entities.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Masareef.BusinessLayer.Services;

public class AuditService
{
    private readonly IAuditLogRepository _repo;
    private readonly IHttpContextAccessor _http;
    private readonly ILogger<AuditService> _logger;

    public AuditService(IAuditLogRepository repo, IHttpContextAccessor http,
        ILogger<AuditService> logger)
    {
        _repo = repo;
        _http = http;
        _logger = logger;
    }

    /// <summary>
    /// التسجيل ثانوي بالنسبة للعملية نفسها — لو فشل الكتابة في الداتابيز بنسجّل الفشل
    /// في ملف الـ log ومنوقفش العملية الأساسية (اليوزر يفضل يقدر يدخل ويشتغل).
    /// </summary>
    public async Task LogAsync(int? userId, string action, string? entityType = null,
        int? entityId = null, string? details = null, bool success = true)
    {
        var ip = _http.HttpContext?.Connection.RemoteIpAddress?.ToString();

        try
        {
            await _repo.AddAsync(new AuditLog
            {
                UserId = userId,
                Action = action,
                EntityType = entityType,
                EntityId = entityId,
                Details = details,
                IpAddress = ip,
                Success = success,
                CreatedAt = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Failed to write audit entry {Action} for user {UserId} on {EntityType} {EntityId}",
                action, userId, entityType, entityId);
        }
    }

    public async Task<Result<List<AuditLog>>> SearchAsync(int? userId, string? action) =>
        Result<List<AuditLog>>.Ok(await _repo.SearchAsync(userId, action));
}
