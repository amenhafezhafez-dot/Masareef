using Masareef.DAL.Repository;
using Masareef.Entities.Common;
using Masareef.Entities.DTOs;
using Masareef.Entities.Models;

namespace Masareef.BusinessLayer.Services;

public class HomeService
{
    private readonly IHomeRepository _repo;

    public HomeService(IHomeRepository repo) => _repo = repo;

    // أنواع التصنيفات
    private const byte KIND_EXPENSE = 1;
    private const byte KIND_INCOME = 2;


    // ═══════════════════ بحث المصاريف ═══════════════════

    public async Task<Result<List<HomeExpenseListDto>>> SearchExpensesAsync(
        int userId, HomeFilterDto filter)
    {
        if (userId <= 0)
            return Result<List<HomeExpenseListDto>>.Fail("Invalid user ID.");

        var error = ValidateFilter(filter);
        if (error is not null)
            return Result<List<HomeExpenseListDto>>.Fail(error);

        if (filter.PaymentMethod.HasValue && filter.PaymentMethod is < 1 or > 4)
            return Result<List<HomeExpenseListDto>>.Fail("Invalid payment method.");

        return Result<List<HomeExpenseListDto>>.Ok(
            await _repo.SearchExpensesAsync(userId, filter));
    }


    // ── اختصارات مريحة ──

    public Task<Result<List<HomeExpenseListDto>>> GetExpensesAsync(
        int userId, int? year = null, int? month = null, int? categoryId = null) =>
        SearchExpensesAsync(userId, new HomeFilterDto
        {
            Year = year,
            Month = month,
            CategoryId = categoryId
        });

    /// <summary>مصاريف الشهر الحالي</summary>
    public Task<Result<List<HomeExpenseListDto>>> GetCurrentMonthExpensesAsync(int userId) =>
        SearchExpensesAsync(userId, new HomeFilterDto
        {
            Year = DateTime.Today.Year,
            Month = DateTime.Today.Month
        });

    /// <summary>مصاريف في نطاق تواريخ</summary>
    public Task<Result<List<HomeExpenseListDto>>> GetExpensesByDateRangeAsync(
        int userId, DateTime? from, DateTime? to) =>
        SearchExpensesAsync(userId, new HomeFilterDto { DateFrom = from, DateTo = to });

    /// <summary>مصاريف في نطاق مبالغ</summary>
    public Task<Result<List<HomeExpenseListDto>>> GetExpensesByAmountRangeAsync(
        int userId, decimal? min, decimal? max) =>
        SearchExpensesAsync(userId, new HomeFilterDto { MinAmount = min, MaxAmount = max });

    /// <summary>بحث نصّي في وصف المصاريف</summary>
    public Task<Result<List<HomeExpenseListDto>>> SearchExpensesByTextAsync(
        int userId, string text) =>
        string.IsNullOrWhiteSpace(text) || text.Trim().Length < 2
            ? Task.FromResult(Result<List<HomeExpenseListDto>>.Fail(
                "Search text must be at least 2 characters."))
            : SearchExpensesAsync(userId, new HomeFilterDto { SearchText = text.Trim() });


    // ═══════════════════ كتابة المصاريف ═══════════════════

    public async Task<Result<int>> AddExpenseAsync(CreateHomeExpenseDto dto)
    {
        if (dto.UserId <= 0)
            return Result<int>.Fail("Invalid user ID.");

        var error = ValidateAmount(dto.Amount);
        if (error is not null) return Result<int>.Fail(error);

        error = ValidateDate(dto.ExpenseDate);
        if (error is not null) return Result<int>.Fail(error);

        if (dto.PaymentMethod is < 1 or > 4)
            return Result<int>.Fail("Invalid payment method.");

        if (!await _repo.CategoryExistsAsync(dto.CategoryId, KIND_EXPENSE))
            return Result<int>.Fail("Category not found or is not an expense category.");

        var expense = new HomeExpense
        {
            UserId = dto.UserId,
            CategoryId = dto.CategoryId,
            Amount = dto.Amount,
            ExpenseDate = dto.ExpenseDate.Date,
            PaymentMethod = dto.PaymentMethod,
            Description = dto.Description?.Trim(),
            CreatedAt = DateTime.UtcNow
        };

        return Result<int>.Ok(await _repo.AddHomeExpenseAsync(expense));
    }


    public async Task<Result> UpdateExpenseAsync(
        int expenseId, int userId, decimal amount, DateTime date,
        int categoryId, byte method, string? description)
    {
        if (expenseId <= 0 || userId <= 0)
            return Result.Fail("Invalid parameters.");

        var error = ValidateAmount(amount);
        if (error is not null) return Result.Fail(error);

        error = ValidateDate(date);
        if (error is not null) return Result.Fail(error);

        if (method is < 1 or > 4)
            return Result.Fail("Invalid payment method.");

        // 🔒 المصروف موجود وتبع اليوزر ده؟ — فحص خفيف
        if (!await _repo.ExpenseExistsAsync(expenseId, userId))
            return Result.Fail("Expense not found.");

        if (!await _repo.CategoryExistsAsync(categoryId, KIND_EXPENSE))
            return Result.Fail("Category not found or is not an expense category.");

        bool ok = await _repo.UpdateHomeExpenseAsync(
            expenseId, userId, amount, date.Date, categoryId, method, description?.Trim());

        return ok ? Result.Ok() : Result.Fail("Update failed.");
    }


    public async Task<Result> DeleteExpenseAsync(int expenseId, int userId)
    {
        if (expenseId <= 0 || userId <= 0)
            return Result.Fail("Invalid parameters.");

        bool ok = await _repo.DeleteHomeExpenseAsync(expenseId, userId);
        return ok ? Result.Ok() : Result.Fail("Expense not found.");
    }


    // ═══════════════════ بحث الدخل ═══════════════════

    public async Task<Result<List<HomeIncomeListDto>>> SearchIncomesAsync(
        int userId, HomeFilterDto filter)
    {
        if (userId <= 0)
            return Result<List<HomeIncomeListDto>>.Fail("Invalid user ID.");

        var error = ValidateFilter(filter);
        if (error is not null)
            return Result<List<HomeIncomeListDto>>.Fail(error);

        return Result<List<HomeIncomeListDto>>.Ok(
            await _repo.SearchIncomesAsync(userId, filter));
    }


    // ── اختصارات ──

    public Task<Result<List<HomeIncomeListDto>>> GetIncomesAsync(
        int userId, int? year = null, int? month = null) =>
        SearchIncomesAsync(userId, new HomeFilterDto { Year = year, Month = month });

    public Task<Result<List<HomeIncomeListDto>>> GetCurrentMonthIncomesAsync(int userId) =>
        SearchIncomesAsync(userId, new HomeFilterDto
        {
            Year = DateTime.Today.Year,
            Month = DateTime.Today.Month
        });

    public Task<Result<List<HomeIncomeListDto>>> GetIncomesByDateRangeAsync(
        int userId, DateTime? from, DateTime? to) =>
        SearchIncomesAsync(userId, new HomeFilterDto { DateFrom = from, DateTo = to });

    public Task<Result<List<HomeIncomeListDto>>> GetIncomesByAmountRangeAsync(
        int userId, decimal? min, decimal? max) =>
        SearchIncomesAsync(userId, new HomeFilterDto { MinAmount = min, MaxAmount = max });

    /// <summary>بحث في المصدر أو الوصف</summary>
    public Task<Result<List<HomeIncomeListDto>>> SearchIncomesByTextAsync(
        int userId, string text) =>
        string.IsNullOrWhiteSpace(text) || text.Trim().Length < 2
            ? Task.FromResult(Result<List<HomeIncomeListDto>>.Fail(
                "Search text must be at least 2 characters."))
            : SearchIncomesAsync(userId, new HomeFilterDto { SearchText = text.Trim() });


    // ═══════════════════ كتابة الدخل ═══════════════════

    public async Task<Result<int>> AddIncomeAsync(CreateHomeIncomeDto dto)
    {
        if (dto.UserId <= 0)
            return Result<int>.Fail("Invalid user ID.");

        var error = ValidateAmount(dto.Amount);
        if (error is not null) return Result<int>.Fail(error);

        error = ValidateDate(dto.IncomeDate);
        if (error is not null) return Result<int>.Fail(error);

        // ⭐ نوع الدخل مش المصروف
        if (!await _repo.CategoryExistsAsync(dto.CategoryId, KIND_INCOME))
            return Result<int>.Fail("Category not found or is not an income category.");

        var income = new HomeIncome
        {
            UserId = dto.UserId,
            CategoryId = dto.CategoryId,
            Amount = dto.Amount,
            IncomeDate = dto.IncomeDate.Date,
            Source = dto.Source?.Trim(),
            Description = dto.Description?.Trim(),
            CreatedAt = DateTime.UtcNow
        };

        return Result<int>.Ok(await _repo.AddHomeIncomeAsync(income));
    }


    public async Task<Result> UpdateIncomeAsync(
        int incomeId, int userId, decimal amount, DateTime date,
        int categoryId, string? source, string? description)
    {
        if (incomeId <= 0 || userId <= 0)
            return Result.Fail("Invalid parameters.");

        var error = ValidateAmount(amount);
        if (error is not null) return Result.Fail(error);

        error = ValidateDate(date);
        if (error is not null) return Result.Fail(error);

        // 🔒 الدخل موجود وتبع اليوزر ده؟
        if (!await _repo.IncomeExistsAsync(incomeId, userId))
            return Result.Fail("Income not found.");

        if (!await _repo.CategoryExistsAsync(categoryId, KIND_INCOME))
            return Result.Fail("Category not found or is not an income category.");

        bool ok = await _repo.UpdateHomeIncomeAsync(
            incomeId, userId, amount, date.Date, categoryId,
            source?.Trim(), description?.Trim());

        return ok ? Result.Ok() : Result.Fail("Update failed.");
    }


    public async Task<Result> DeleteIncomeAsync(int incomeId, int userId)
    {
        if (incomeId <= 0 || userId <= 0)
            return Result.Fail("Invalid parameters.");

        bool ok = await _repo.DeleteHomeIncomeAsync(incomeId, userId);
        return ok ? Result.Ok() : Result.Fail("Income not found.");
    }


    // ═══════════════════ فحوصات مشتركة ═══════════════════
    // بترجّع رسالة الخطأ، أو null لو كله تمام

    private static string? ValidateAmount(decimal amount)
    {
        if (amount <= 0)
            return "Amount must be greater than zero.";

        if (amount > 100_000_000)
            return "Amount is unrealistically large.";

        return null;
    }


    private static string? ValidateDate(DateTime date)
    {
        if (date.Date > DateTime.Today)
            return "Date cannot be in the future.";

        if (date.Year < 2000)
            return "Date is too old.";

        return null;
    }


    private static string? ValidateFilter(HomeFilterDto f)
    {
        if (f.Month.HasValue && f.Month is < 1 or > 12)
            return "Month must be between 1 and 12.";

        if (f.Year.HasValue && f.Year is < 2000 or > 2100)
            return "Invalid year.";

        if (f.MinAmount.HasValue && f.MaxAmount.HasValue && f.MinAmount > f.MaxAmount)
            return "Min amount cannot exceed max amount.";

        if (f.DateFrom.HasValue && f.DateTo.HasValue && f.DateFrom > f.DateTo)
            return "Invalid date range.";

        return null;
    }
}