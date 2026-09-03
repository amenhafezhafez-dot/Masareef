using Microsoft.EntityFrameworkCore;
using Masareef.DAL.Context;
using Masareef.Entities.DTOs;
using Masareef.Entities.Models;

namespace Masareef.DAL.Repository;

public class CategoryRepository : ICategoryRepository
{
    private readonly AppDbContext _context;

    public CategoryRepository(AppDbContext context) => _context = context;


    // ═══════════════════ قراءة ═══════════════════

    public async Task<List<CategoryDto>> GetAsync(byte scope, byte kind, int userId) =>
        await _context.Categories
            .AsNoTracking()
            .Where(c => c.CategoryKind == kind
                     && (c.CategoryScope == scope || c.CategoryScope == 3)
                     && (c.UserId == null || c.UserId == userId))
            .Select(c => new CategoryDto
            {
                CategoryId = c.CategoryId,
                CategoryName = c.CategoryName,
                Icon = c.Icon,
                CategoryKind = c.CategoryKind,
                CategoryScope = c.CategoryScope
            })
            .OrderBy(c => c.CategoryName)
            .ToListAsync();


    public async Task<CategoryDto?> GetByIdAsync(int categoryId, int userId) =>
        await _context.Categories
            .AsNoTracking()
            .Where(c => c.CategoryId == categoryId
                     && (c.UserId == null || c.UserId == userId))
            .Select(c => new CategoryDto
            {
                CategoryId = c.CategoryId,
                CategoryName = c.CategoryName,
                Icon = c.Icon,
                CategoryKind = c.CategoryKind,
                CategoryScope = c.CategoryScope
            })
            .FirstOrDefaultAsync();


    // ═══════════════════ كتابة ═══════════════════

    public async Task<int> AddAsync(int userId, string name, string? icon, byte kind, byte scope)
    {
        var entity = new Category
        {
            UserId = userId,              // 🔒 تصنيف خاص باليوزر ده
            CategoryName = name,
            Icon = icon,
            CategoryKind = kind,
            CategoryScope = scope
        };

        _context.Categories.Add(entity);
        await _context.SaveChangesAsync();

        return entity.CategoryId;         // بنرجّع الـ id مش true
    }


    public async Task<bool> UpdateAsync(int categoryId, int userId,
        string name, string? icon, byte kind, byte scope)
    {
        // 🔒 التصنيفات العامة (UserId == null) مينفعش تتعدّل
        var entity = await _context.Categories
            .FirstOrDefaultAsync(c => c.CategoryId == categoryId && c.UserId == userId);

        if (entity is null) return false;

        entity.CategoryName = name;
        entity.Icon = icon;
        entity.CategoryKind = kind;
        entity.CategoryScope = scope;

        // مفيش Add هنا — الـ change tracker بيلاحظ التعديل لوحده
        return await _context.SaveChangesAsync() > 0;
    }


    public async Task<bool> DeleteAsync(int categoryId, int userId)
    {
        var entity = await _context.Categories
            .FirstOrDefaultAsync(c => c.CategoryId == categoryId && c.UserId == userId);

        if (entity is null) return false;

        _context.Categories.Remove(entity);
        return await _context.SaveChangesAsync() > 0;
    }


    // ═══════════════════ فحوصات ═══════════════════

    public async Task<bool> ExistsAsync(int categoryId, byte kind) =>
        await _context.Categories
            .AsNoTracking()
            .AnyAsync(c => c.CategoryId == categoryId && c.CategoryKind == kind);


    /// <summary>🔒 مملوك لليوزر؟ التصنيفات العامة بترجّع false — محدش يعدّلها</summary>
    public async Task<bool> IsOwnedByUserAsync(int categoryId, int userId) =>
        await _context.Categories
            .AsNoTracking()
            .AnyAsync(c => c.CategoryId == categoryId && c.UserId == userId);


    /// <summary>⭐ مستخدم في أي عملية؟ — بيمنع الحذف اللي هيكسر البيانات</summary>
    public async Task<bool> IsInUseAsync(int categoryId) =>
        await _context.HomeExpenses.AnyAsync(e => e.CategoryId == categoryId)
        || await _context.HomeIncomes.AnyAsync(i => i.CategoryId == categoryId)
        || await _context.BusinessExpenses.AnyAsync(e => e.CategoryId == categoryId)
        || await _context.BusinessIncomes.AnyAsync(i => i.CategoryId == categoryId);


    public async Task<bool> NameExistsAsync(
        string name, byte kind, byte scope, int userId, int? excludeId = null)
    {
        var query = _context.Categories
            .AsNoTracking()
            .Where(c => c.CategoryName == name
                     && c.CategoryKind == kind
                     && c.CategoryScope == scope
                     && c.UserId == userId);

        // وقت التعديل، نستثني التصنيف نفسه من فحص التكرار
        if (excludeId.HasValue)
            query = query.Where(c => c.CategoryId != excludeId.Value);

        return await query.AnyAsync();
    }
}