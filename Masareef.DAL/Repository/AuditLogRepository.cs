using Masareef.DAL.Context;
using Masareef.Entities.Models;
using Microsoft.EntityFrameworkCore;

namespace Masareef.DAL.Repository;

public class AuditLogRepository : IAuditLogRepository
{
    private readonly AppDbContext _context;

    public AuditLogRepository(AppDbContext context) => _context = context;

    public async Task AddAsync(AuditLog entry)
    {
        _context.AuditLogs.Add(entry);
        await _context.SaveChangesAsync();
    }

    public async Task<List<AuditLog>> SearchAsync(int? userId, string? action, int take = 200)
    {
        var q = _context.AuditLogs.AsNoTracking();

        if (userId.HasValue)
            q = q.Where(a => a.UserId == userId);

        if (!string.IsNullOrWhiteSpace(action))
            q = q.Where(a => a.Action == action);

        return await q
            .OrderByDescending(a => a.Id)
            .Take(take)
            .ToListAsync();
    }
}
