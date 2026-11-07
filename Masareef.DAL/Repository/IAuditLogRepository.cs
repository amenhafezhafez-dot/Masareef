using Masareef.Entities.Models;

namespace Masareef.DAL.Repository;

public interface IAuditLogRepository
{
    Task AddAsync(AuditLog entry);

    /// <summary>للعرض في لوحة الـ Admin — فلترة اختيارية باليوزر أو نوع الحدث</summary>
    Task<List<AuditLog>> SearchAsync(int? userId, string? action, int take = 200);
}
