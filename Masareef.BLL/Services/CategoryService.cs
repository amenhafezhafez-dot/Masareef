using Masareef.DAL.Repository;
using Masareef.Entities.Common;
using Masareef.Entities.DTOs;

namespace Masareef.BusinessLayer.Services;

public class CategoryService
{
    private readonly ICategoryRepository _repo;

    public CategoryService(ICategoryRepository repo) => _repo = repo;


    // ═══════════════════ قراءة ═══════════════════

    /// <summary>scope: 1=بيزنس 2=بيت 3=الاتنين | kind: 1=مصروف 2=دخل</summary>
    public async Task<Result<List<CategoryDto>>> GetAsync(byte scope, byte kind, int userId)
    {
        if (userId <= 0)
            return Result<List<CategoryDto>>.Fail("Invalid user ID.");

        var error = ValidateScopeAndKind(scope, kind);
        if (error is not null)
            return Result<List<CategoryDto>>.Fail(error);

        return Result<List<CategoryDto>>.Ok(await _repo.GetAsync(scope, kind, userId));
    }


    public async Task<Result<CategoryDto>> GetByIdAsync(int categoryId, int userId)
    {
        if (categoryId <= 0)
            return Result<CategoryDto>.Fail("Invalid category ID.");

        var dto = await _repo.GetByIdAsync(categoryId, userId);

        return dto is null
            ? Result<CategoryDto>.Fail("Category not found.")
            : Result<CategoryDto>.Ok(dto);
    }


    // ── اختصارات مريحة ──

    public Task<Result<List<CategoryDto>>> GetHomeExpenseCategoriesAsync(int userId) =>
        GetAsync(scope: 2, kind: 1, userId);

    public Task<Result<List<CategoryDto>>> GetHomeIncomeCategoriesAsync(int userId) =>
        GetAsync(scope: 2, kind: 2, userId);

    public Task<Result<List<CategoryDto>>> GetBusinessExpenseCategoriesAsync(int userId) =>
        GetAsync(scope: 1, kind: 1, userId);

    public Task<Result<List<CategoryDto>>> GetBusinessIncomeCategoriesAsync(int userId) =>
        GetAsync(scope: 1, kind: 2, userId);


    // ═══════════════════ إضافة ═══════════════════

    public async Task<Result<int>> CreateAsync(
        int userId, string name, string? icon, byte kind, byte scope)
    {
        if (userId <= 0)
            return Result<int>.Fail("Invalid user ID.");

        var error = ValidateName(name);
        if (error is not null) return Result<int>.Fail(error);

        error = ValidateScopeAndKind(scope, kind);
        if (error is not null) return Result<int>.Fail(error);

        // ⭐ الاسم متكرر لنفس اليوزر ونفس النوع؟
        if (await _repo.NameExistsAsync(name.Trim(), kind, scope, userId))
            return Result<int>.Fail("A category with this name already exists.");

        int newId = await _repo.AddAsync(
            userId, name.Trim(), icon?.Trim(), kind, scope);

        return Result<int>.Ok(newId);
    }


    // ═══════════════════ تعديل ═══════════════════

    public async Task<Result> UpdateAsync(
        int categoryId, int userId, string name, string? icon, byte kind, byte scope)
    {
        if (categoryId <= 0 || userId <= 0)
            return Result.Fail("Invalid parameters.");

        var error = ValidateName(name);
        if (error is not null) return Result.Fail(error);

        error = ValidateScopeAndKind(scope, kind);
        if (error is not null) return Result.Fail(error);

        // 🔒 التصنيف مملوك لليوزر ده؟ (العامة مش قابلة للتعديل)
        if (!await _repo.IsOwnedByUserAsync(categoryId, userId))
            return Result.Fail("Category not found or is a system category.");

        // ⭐ الاسم متكرر؟ — مع استثناء التصنيف نفسه
        if (await _repo.NameExistsAsync(name.Trim(), kind, scope, userId, excludeId: categoryId))
            return Result.Fail("Another category with this name already exists.");

        bool ok = await _repo.UpdateAsync(
            categoryId, userId, name.Trim(), icon?.Trim(), kind, scope);

        return ok ? Result.Ok() : Result.Fail("Update failed.");
    }


    // ═══════════════════ حذف ═══════════════════

    public async Task<Result> DeleteAsync(int categoryId, int userId)
    {
        if (categoryId <= 0 || userId <= 0)
            return Result.Fail("Invalid parameters.");

        // 🔒 مملوك لليوزر؟
        if (!await _repo.IsOwnedByUserAsync(categoryId, userId))
            return Result.Fail("Category not found or is a system category.");

        // 🔴 القاعدة المهمة — مينفعش تمسح تصنيف مستخدم في عمليات
        if (await _repo.IsInUseAsync(categoryId))
            return Result.Fail(
                "This category is used in existing transactions and cannot be deleted.");

        bool ok = await _repo.DeleteAsync(categoryId, userId);

        return ok ? Result.Ok() : Result.Fail("Delete failed.");
    }


    // ═══════════════════ فحوصات مشتركة ═══════════════════

    private static string? ValidateName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return "Category name is required.";

        if (name.Trim().Length < 2)
            return "Category name must be at least 2 characters.";

        if (name.Trim().Length > 100)
            return "Category name is too long (max 100).";

        return null;
    }


    private static string? ValidateScopeAndKind(byte scope, byte kind)
    {
        if (scope is < 1 or > 3)
            return "Invalid scope (1=Business, 2=Home, 3=Both).";

        if (kind is < 1 or > 2)
            return "Invalid kind (1=Expense, 2=Income).";

        return null;
    }
}