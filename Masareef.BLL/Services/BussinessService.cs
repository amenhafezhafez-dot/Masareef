using Masareef.DAL.Repository;
using Masareef.Entities.Common;
using Masareef.Entities.DTOs;
using Masareef.Entities.Models;

namespace Masareef.BusinessLayer.Services;

public class BusinessService
{
    private readonly IBusinessRepository _repo;

    public BusinessService(IBusinessRepository repo) => _repo = repo;


    // ═══════════════════ المحلات — قراءة ═══════════════════

    /// <summary>⭐ البحث الأساسي — كل الاختصارات بتمر عليه</summary>
    public async Task<Result<List<BusinessListDto>>> SearchAsync(
        int userId, BusinessFilterDto filter, CancellationToken ct = default)
    {
        if (userId <= 0)
            return Result<List<BusinessListDto>>.Fail("Invalid user ID.");

        if (filter.CreatedFrom.HasValue && filter.CreatedTo.HasValue
            && filter.CreatedFrom > filter.CreatedTo)
            return Result<List<BusinessListDto>>.Fail("Invalid date range.");

        if (!string.IsNullOrWhiteSpace(filter.Currency) && filter.Currency.Trim().Length != 3)
            return Result<List<BusinessListDto>>.Fail("Currency must be 3 letters.");

        return Result<List<BusinessListDto>>.Ok(
            await _repo.SearchAsync(userId, filter, ct));
    }


    // ── اختصارات ──

    /// <summary>المحلات النشطة</summary>
    public Task<Result<List<BusinessListDto>>> GetAllAsync(
        int userId, CancellationToken ct = default) =>
        SearchAsync(userId, new BusinessFilterDto { IsActive = true }, ct);


    /// <summary>المحلات المعطّلة</summary>
    public Task<Result<List<BusinessListDto>>> GetInactiveAsync(
        int userId, CancellationToken ct = default) =>
        SearchAsync(userId, new BusinessFilterDto { IsActive = false }, ct);


    /// <summary>بحث بالاسم</summary>
    public Task<Result<List<BusinessListDto>>> SearchByNameAsync(
        int userId, string name, CancellationToken ct = default) =>
        string.IsNullOrWhiteSpace(name) || name.Trim().Length < 2
            ? Task.FromResult(Result<List<BusinessListDto>>.Fail(
                "Search text must be at least 2 characters."))
            : SearchAsync(userId, new BusinessFilterDto { Name = name.Trim() }, ct);


    public async Task<Result<BusinessListDto>> GetByIdAsync(
        int businessId, int userId, CancellationToken ct = default)
    {
        if (businessId <= 0)
            return Result<BusinessListDto>.Fail("Invalid business ID.");

        var dto = await _repo.GetDetailsAsync(businessId, userId, ct);

        return dto is null
            ? Result<BusinessListDto>.Fail("Business not found.")
            : Result<BusinessListDto>.Ok(dto);
    }


    // ═══════════════════ الأرباح ═══════════════════

    /// <summary>أرباح محل واحد في شهر معيّن</summary>
    public async Task<Result<BusinessProfitDto>> GetProfitAsync(
        int businessId, int userId, int year, int month, CancellationToken ct = default)
    {
        if (businessId <= 0)
            return Result<BusinessProfitDto>.Fail("Invalid business ID.");

        if (!IsValidPeriod(year, month, out string error))
            return Result<BusinessProfitDto>.Fail(error);

        var dto = await _repo.GetProfitForOneBusinessInMounthAsync(
            businessId, userId, year, month, ct);

        return dto is null
            ? Result<BusinessProfitDto>.Fail("Business not found.")
            : Result<BusinessProfitDto>.Ok(dto);
    }


    /// <summary>أرباح كل المحلات — للمقارنة</summary>
    public async Task<Result<List<BusinessProfitDto>>> GetAllProfitsAsync(
        int userId, int year, int month, CancellationToken ct = default)
    {
        if (userId <= 0)
            return Result<List<BusinessProfitDto>>.Fail("Invalid user ID.");

        if (!IsValidPeriod(year, month, out string error))
            return Result<List<BusinessProfitDto>>.Fail(error);

        return Result<List<BusinessProfitDto>>.Ok(
            await _repo.GetProfitsForAllBussinessAsync(userId, year, month, ct));
    }


    /// <summary>أرباح المحل منذ إنشائه</summary>
    public async Task<Result<BusinessProfitDto>> GetLifetimeProfitAsync(
        int businessId, int userId)
    {
        if (businessId <= 0)
            return Result<BusinessProfitDto>.Fail("Invalid business ID.");

        var dto = await _repo.GetProfitForBusinessEverAsync(businessId, userId);

        return dto is null
            ? Result<BusinessProfitDto>.Fail("Business not found.")
            : Result<BusinessProfitDto>.Ok(dto);
    }


    // ═══════════════════ المحلات — كتابة ═══════════════════

    public async Task<Result<int>> CreateAsync(
        int userId, string name, string? description, string currency)
    {
        if (userId <= 0)
            return Result<int>.Fail("Invalid user ID.");

        var error = ValidateBusinessInput(name, currency);
        if (error is not null) return Result<int>.Fail(error);

        // ⭐ اسم مكرر لنفس اليوزر؟
        if (await _repo.NameExistsAsync(name.Trim(), userId))
            return Result<int>.Fail("A business with this name already exists.");

        var business = new Business
        {
            UserId = userId,
            BusinessName = name.Trim(),
            Description = description?.Trim(),
            Currency = currency.Trim().ToUpperInvariant(),

            // من النظام
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        return Result<int>.Ok(await _repo.AddBusinessAsync(business));
    }


    public async Task<Result> UpdateAsync(
        int businessId, int userId, string name, string? description, string currency)
    {
        if (businessId <= 0 || userId <= 0)
            return Result.Fail("Invalid parameters.");

        var error = ValidateBusinessInput(name, currency);
        if (error is not null) return Result.Fail(error);

        // 🔒 المحل تبع اليوزر ده؟
        if (!await _repo.BusinessBelongsToUserAsync(businessId, userId))
            return Result.Fail("Business not found.");

        // ⭐ الاسم مكرر؟ — مع استثناء المحل نفسه
        if (await _repo.NameExistsAsync(name.Trim(), userId, excludeId: businessId))
            return Result.Fail("Another business with this name already exists.");

        bool ok = await _repo.UpdateBusinessAsync(
            businessId, userId, name.Trim(),
            description?.Trim(), currency.Trim().ToUpperInvariant());

        return ok ? Result.Ok() : Result.Fail("Update failed.");
    }


    /// <summary>تعطيل مش حذف — عشان التاريخ المالي يفضل محفوظ</summary>
    public async Task<Result> DeactivateAsync(int businessId, int userId)
    {
        if (businessId <= 0 || userId <= 0)
            return Result.Fail("Invalid parameters.");

        var business = await _repo.GetDetailsAsync(businessId, userId);

        if (business is null)
            return Result.Fail("Business not found.");

        if (!business.IsActive)
            return Result.Fail("This business is already deactivated.");

        bool ok = await _repo.DeactivateBusinessAsync(businessId, userId);

        return ok ? Result.Ok() : Result.Fail("Deactivation failed.");
    }


    public async Task<Result> ReactivateAsync(int businessId, int userId)
    {
        if (businessId <= 0 || userId <= 0)
            return Result.Fail("Invalid parameters.");

        var business = await _repo.GetDetailsAsync(businessId, userId);

        if (business is null)
            return Result.Fail("Business not found.");

        if (business.IsActive)
            return Result.Fail("This business is already active.");

        bool ok = await _repo.ReactivateBusinessAsync(businessId, userId);

        return ok ? Result.Ok() : Result.Fail("Reactivation failed.");
    }


    // ═══════════════════ المصاريف ═══════════════════

    public async Task<Result<List<BusinessTransactionDto>>> GetExpensesAsync(
        int businessId, int userId, BusinessTransactionFilterDto filter,
        CancellationToken ct = default)
    {
        // 🔒 المحل تبع اليوزر ده؟
        if (!await _repo.BusinessBelongsToUserAsync(businessId, userId))
            return Result<List<BusinessTransactionDto>>.Fail("Business not found.");

        var error = ValidateTransactionFilter(filter);
        if (error is not null)
            return Result<List<BusinessTransactionDto>>.Fail(error);

        return Result<List<BusinessTransactionDto>>.Ok(
            await _repo.SearchExpensesAsync(businessId, userId, filter, ct));
    }


    public async Task<Result<int>> AddExpenseAsync(CreateBusinessExpenseDto dto, int userId)
    {
        if (!await _repo.BusinessBelongsToUserAsync(dto.BusinessId, userId))
            return Result<int>.Fail("Business not found.");

        var error = ValidateAmount(dto.Amount);
        if (error is not null) return Result<int>.Fail(error);

        error = ValidateDate(dto.ExpenseDate);
        if (error is not null) return Result<int>.Fail(error);

        if (dto.PaymentMethod is < 1 or > 4)
            return Result<int>.Fail("Invalid payment method.");

        var expense = new BusinessExpense
        {
            BusinessId = dto.BusinessId,
            CategoryId = dto.CategoryId,
            Amount = dto.Amount,
            ExpenseDate = dto.ExpenseDate.Date,
            PaymentMethod = dto.PaymentMethod,
            Description = dto.Description?.Trim(),
            CreatedAt = DateTime.UtcNow
        };

        return Result<int>.Ok(await _repo.AddBusinessExpenseAsync(expense));
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

        // 🔒 المصروف تبع محل تبع اليوزر ده؟
        if (!await _repo.ExpenseBelongsToUserAsync(expenseId, userId))
            return Result.Fail("Expense not found.");

        bool ok = await _repo.UpdateBusinessExpenseAsync(
            expenseId, userId, amount, date.Date, categoryId, method, description?.Trim());

        return ok ? Result.Ok() : Result.Fail("Update failed.");
    }


    public async Task<Result> DeleteExpenseAsync(int expenseId, int userId)
    {
        if (expenseId <= 0 || userId <= 0)
            return Result.Fail("Invalid parameters.");

        bool ok = await _repo.DeleteBusinessExpenseAsync(expenseId, userId);

        return ok ? Result.Ok() : Result.Fail("Expense not found.");
    }


    // ═══════════════════ الدخل ═══════════════════

    public async Task<Result<List<BusinessTransactionDto>>> GetIncomesAsync(
        int businessId, int userId, BusinessTransactionFilterDto filter,
        CancellationToken ct = default)
    {
        if (!await _repo.BusinessBelongsToUserAsync(businessId, userId))
            return Result<List<BusinessTransactionDto>>.Fail("Business not found.");

        var error = ValidateTransactionFilter(filter);
        if (error is not null)
            return Result<List<BusinessTransactionDto>>.Fail(error);

        return Result<List<BusinessTransactionDto>>.Ok(
            await _repo.SearchIncomesAsync(businessId, userId, filter, ct));
    }


    public async Task<Result<int>> AddIncomeAsync(CreateBusinessIncomeDto dto, int userId)
    {
        if (!await _repo.BusinessBelongsToUserAsync(dto.BusinessId, userId))
            return Result<int>.Fail("Business not found.");

        var error = ValidateAmount(dto.Amount);
        if (error is not null) return Result<int>.Fail(error);

        error = ValidateDate(dto.IncomeDate);
        if (error is not null) return Result<int>.Fail(error);

        var income = new BusinessIncome
        {
            BusinessId = dto.BusinessId,
            CategoryId = dto.CategoryId,
            Amount = dto.Amount,
            IncomeDate = dto.IncomeDate.Date,
            Source = dto.Source?.Trim(),
            Description = dto.Description?.Trim(),
            CreatedAt = DateTime.UtcNow
        };

        return Result<int>.Ok(await _repo.AddBusinessIncomeAsync(income));
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

        // 🔒 الدخل تبع محل تبع اليوزر ده؟
        if (!await _repo.IncomeBelongsToUserAsync(incomeId, userId))
            return Result.Fail("Income not found.");

        bool ok = await _repo.UpdateBusinessIncomeAsync(
            incomeId, userId, amount, date.Date, categoryId,
            source?.Trim(), description?.Trim());

        return ok ? Result.Ok() : Result.Fail("Update failed.");
    }


    public async Task<Result> DeleteIncomeAsync(int incomeId, int userId)
    {
        if (incomeId <= 0 || userId <= 0)
            return Result.Fail("Invalid parameters.");

        bool ok = await _repo.DeleteBusinessIncomeAsync(incomeId, userId);

        return ok ? Result.Ok() : Result.Fail("Income not found.");
    }


    // ═══════════════════ فحوصات مشتركة ═══════════════════

    private static string? ValidateBusinessInput(string name, string currency)
    {
        if (string.IsNullOrWhiteSpace(name))
            return "Business name is required.";

        if (name.Trim().Length < 3)
            return "Business name must be at least 3 characters.";

        if (name.Trim().Length > 100)
            return "Business name is too long (max 100).";

        if (string.IsNullOrWhiteSpace(currency) || currency.Trim().Length != 3)
            return "Currency must be exactly 3 letters (e.g. EGP).";

        return null;
    }


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


    private static string? ValidateTransactionFilter(BusinessTransactionFilterDto f)
    {
        if (f.Month.HasValue && f.Month is < 1 or > 12)
            return "Month must be between 1 and 12.";

        if (f.Year.HasValue && f.Year is < 2000 or > 2100)
            return "Invalid year.";

        if (f.MinAmount.HasValue && f.MaxAmount.HasValue && f.MinAmount > f.MaxAmount)
            return "Min amount cannot exceed max amount.";

        if (f.DateFrom.HasValue && f.DateTo.HasValue && f.DateFrom > f.DateTo)
            return "Invalid date range.";

        if (f.PaymentMethod.HasValue && f.PaymentMethod is < 1 or > 4)
            return "Invalid payment method.";

        return null;
    }


    /// <summary>مستخدمة كمان في ReportService — سيبها internal static</summary>
    internal static bool IsValidPeriod(int year, int month, out string error)
    {
        error = "";

        if (year is < 2000 or > 2100)
        {
            error = "Invalid year.";
            return false;
        }

        if (month is < 1 or > 12)
        {
            error = "Month must be between 1 and 12.";
            return false;
        }

        return true;
    }
}