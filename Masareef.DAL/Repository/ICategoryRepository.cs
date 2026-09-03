using Masareef.Entities.DTOs;

namespace Masareef.DAL.Repository;

public interface ICategoryRepository
{
    /// <summary>scope: 1=بيزنس 2=بيت 3=الاتنين | kind: 1=مصروف 2=دخل</summary>
    Task<List<CategoryDto>> GetAsync(byte scope, byte kind, int userId);

    /// <summary>تصنيف واحد بالتفصيل</summary>
    Task<CategoryDto?> GetByIdAsync(int categoryId, int userId);

    Task<int> AddAsync(int userId, string name, string? icon, byte kind, byte scope);

    Task<bool> UpdateAsync(int categoryId, int userId,
        string name, string? icon, byte kind, byte scope);

    Task<bool> DeleteAsync(int categoryId, int userId);

    // ── فحوصات ──
    Task<bool> ExistsAsync(int categoryId, byte kind);

    /// <summary>🔒 التصنيف مملوك لليوزر ده؟ (التصنيفات العامة مش مملوكة لحد)</summary>
    Task<bool> IsOwnedByUserAsync(int categoryId, int userId);

    /// <summary>هل التصنيف مستخدم في أي عملية؟ — لمنع الحذف</summary>
    Task<bool> IsInUseAsync(int categoryId);

    /// <summary>اسم مكرر لنفس اليوزر ونفس النوع؟</summary>
    Task<bool> NameExistsAsync(string name, byte kind, byte scope, int userId, int? excludeId = null);
}