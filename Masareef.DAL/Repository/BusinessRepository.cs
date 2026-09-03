using Masareef.DAL.Context;
using Masareef.Entities.DTOs;
using Masareef.Entities.Models;
using Microsoft.EntityFrameworkCore;

namespace Masareef.DAL.Repository;

public class BusinessRepository : IBusinessRepository
{
    private readonly AppDbContext _context;

    public BusinessRepository(AppDbContext context) => _context = context;


    // ═══════════════════ المحلات — قراءة ═══════════════════

    /// <summary>⭐ البحث المرن — بيغطي كل الحالات</summary>
    public async Task<List<BusinessListDto>> SearchAsync(
        int userId, BusinessFilterDto f, CancellationToken ct = default)
    {
        var query = _context.Businesses
            .AsNoTracking()
            .Where(b => b.UserId == userId);

        // الافتراضي: النشطة بس. لو IsActive = null، بيجيب الكل
        if (f.IsActive.HasValue)
            query = query.Where(b => b.IsActive == f.IsActive.Value);

        if (!string.IsNullOrWhiteSpace(f.Name))
            query = query.Where(b => b.BusinessName.Contains(f.Name));

        if (!string.IsNullOrWhiteSpace(f.Currency))
            query = query.Where(b => b.Currency == f.Currency);

        if (f.CreatedFrom.HasValue)
            query = query.Where(b => b.CreatedAt >= f.CreatedFrom.Value);

        if (f.CreatedTo.HasValue)
            query = query.Where(b => b.CreatedAt <= f.CreatedTo.Value);

        return await query
            .Select(b => new BusinessListDto
            {
                BusinessId = b.BusinessId,
                BusinessName = b.BusinessName,
                Description = b.Description,
                Currency = b.Currency,
                CreatedAt = b.CreatedAt,
                IsActive = b.IsActive
            })
            .OrderBy(b => b.BusinessName)
            .ToListAsync(ct);
    }


    public async Task<BusinessListDto?> GetDetailsAsync(
        int businessId, int userId, CancellationToken ct = default) =>
        await _context.Businesses
            .AsNoTracking()
            .Where(b => b.BusinessId == businessId && b.UserId == userId)
            .Select(b => new BusinessListDto
            {
                BusinessId = b.BusinessId,
                BusinessName = b.BusinessName,
                Description = b.Description,
                Currency = b.Currency,
                CreatedAt = b.CreatedAt,
                IsActive = b.IsActive
            })
            .FirstOrDefaultAsync(ct);


    public async Task<Business?> GetEntityForUpdateAsync(int businessId, int userId) =>
        await _context.Businesses
            .FirstOrDefaultAsync(b => b.BusinessId == businessId && b.UserId == userId);


    // ═══════════════════ الأرباح ═══════════════════

    public async Task<BusinessProfitDto?> GetProfitForOneBusinessInMounthAsync(
        int businessId, int userId, int year, int month, CancellationToken ct = default) =>
        await _context.Businesses
            .AsNoTracking()
            .Where(b => b.BusinessId == businessId && b.UserId == userId)
            .Select(b => new BusinessProfitDto
            {
                BusinessId = b.BusinessId,
                BusinessName = b.BusinessName,
                Year = year,
                Month = month,

                TotalIncome = b.BusinessIncomes
                    .Where(i => i.IncomeDate.Year == year && i.IncomeDate.Month == month)
                    .Sum(i => (decimal?)i.Amount) ?? 0,

                TotalExpense = b.BusinessExpenses
                    .Where(e => e.ExpenseDate.Year == year && e.ExpenseDate.Month == month)
                    .Sum(e => (decimal?)e.Amount) ?? 0,

                IncomeCount = b.BusinessIncomes
                    .Count(i => i.IncomeDate.Year == year && i.IncomeDate.Month == month),

                ExpenseCount = b.BusinessExpenses
                    .Count(e => e.ExpenseDate.Year == year && e.ExpenseDate.Month == month)
            })
            .FirstOrDefaultAsync(ct);


    public async Task<List<BusinessProfitDto>> GetProfitsForAllBussinessAsync(
        int userId, int year, int month, CancellationToken ct = default) =>
        await _context.Businesses
            .AsNoTracking()
            .Where(b => b.UserId == userId && b.IsActive)
            .Select(b => new BusinessProfitDto
            {
                BusinessId = b.BusinessId,
                BusinessName = b.BusinessName,
                Year = year,
                Month = month,

                TotalIncome = b.BusinessIncomes
                    .Where(i => i.IncomeDate.Year == year && i.IncomeDate.Month == month)
                    .Sum(i => (decimal?)i.Amount) ?? 0,

                TotalExpense = b.BusinessExpenses
                    .Where(e => e.ExpenseDate.Year == year && e.ExpenseDate.Month == month)
                    .Sum(e => (decimal?)e.Amount) ?? 0,

                IncomeCount = b.BusinessIncomes
                    .Count(i => i.IncomeDate.Year == year && i.IncomeDate.Month == month),

                ExpenseCount = b.BusinessExpenses
                    .Count(e => e.ExpenseDate.Year == year && e.ExpenseDate.Month == month)
            })
            .OrderBy(b => b.BusinessName)
            .ToListAsync(ct);


    public async Task<BusinessProfitDto?> GetProfitForBusinessEverAsync(
        int businessId, int userId) =>
        await _context.Businesses
            .AsNoTracking()
            .Where(b => b.BusinessId == businessId && b.UserId == userId)
            .Select(b => new BusinessProfitDto
            {
                BusinessId = b.BusinessId,
                BusinessName = b.BusinessName,
                createdAt = b.CreatedAt,
                TotalIncome = b.BusinessIncomes.Sum(i => (decimal?)i.Amount) ?? 0,
                TotalExpense = b.BusinessExpenses.Sum(e => (decimal?)e.Amount) ?? 0,
                IncomeCount = b.BusinessIncomes.Count(),
                ExpenseCount = b.BusinessExpenses.Count()
            })
            .FirstOrDefaultAsync();


    // ═══════════════════ المحلات — كتابة ═══════════════════

    public async Task<int> AddBusinessAsync(Business business)
    {
        _context.Businesses.Add(business);
        await _context.SaveChangesAsync();
        return business.BusinessId;
    }


    public async Task<bool> UpdateBusinessAsync(int businessId, int userId,
        string name, string? description, string currency)
    {
        var business = await _context.Businesses
            .FirstOrDefaultAsync(b => b.BusinessId == businessId && b.UserId == userId);

        if (business is null) return false;

        business.BusinessName = name;
        business.Description = description;
        business.Currency = currency;

        return await _context.SaveChangesAsync() > 0;
    }


    public async Task<bool> DeactivateBusinessAsync(int businessId, int userId)
    {
        var b = await _context.Businesses
            .FirstOrDefaultAsync(x => x.BusinessId == businessId && x.UserId == userId);

        if (b is null) return false;

        b.IsActive = false;             // soft delete — التاريخ المالي يفضل محفوظ
        return await _context.SaveChangesAsync() > 0;
    }


    /// <summary>إعادة تفعيل محل معطّل</summary>
    public async Task<bool> ReactivateBusinessAsync(int businessId, int userId)
    {
        var b = await _context.Businesses
            .FirstOrDefaultAsync(x => x.BusinessId == businessId && x.UserId == userId);

        if (b is null) return false;

        b.IsActive = true;
        return await _context.SaveChangesAsync() > 0;
    }


    // ═══════════════════ العمليات — قراءة ═══════════════════

    public async Task<List<BusinessTransactionDto>> SearchExpensesAsync(
        int businessId, int userId, BusinessTransactionFilterDto f,
        CancellationToken ct = default)
    {
        var query = _context.BusinessExpenses
            .AsNoTracking()
            .Where(e => e.BusinessId == businessId && e.Business.UserId == userId);

        if (f.Year.HasValue) query = query.Where(e => e.ExpenseDate.Year == f.Year.Value);
        if (f.Month.HasValue) query = query.Where(e => e.ExpenseDate.Month == f.Month.Value);
        if (f.CategoryId.HasValue) query = query.Where(e => e.CategoryId == f.CategoryId.Value);
        if (f.DateFrom.HasValue) query = query.Where(e => e.ExpenseDate >= f.DateFrom.Value);
        if (f.DateTo.HasValue) query = query.Where(e => e.ExpenseDate <= f.DateTo.Value);
        if (f.MinAmount.HasValue) query = query.Where(e => e.Amount >= f.MinAmount.Value);
        if (f.MaxAmount.HasValue) query = query.Where(e => e.Amount <= f.MaxAmount.Value);

        if (f.PaymentMethod.HasValue)
            query = query.Where(e => e.PaymentMethod == f.PaymentMethod.Value);

        if (!string.IsNullOrWhiteSpace(f.SearchText))
            query = query.Where(e => e.Description != null
                                  && e.Description.Contains(f.SearchText));

        return await query
            .Select(e => new BusinessTransactionDto
            {
                Id = e.ExpenseId,
                Amount = e.Amount,
                Date = e.ExpenseDate,
                Description = e.Description,
                PaymentMethod = e.PaymentMethod,
                CategoryId = e.CategoryId,
                CategoryName = e.Category.CategoryName
            })
            .OrderByDescending(e => e.Date)
            .ThenByDescending(e => e.Id)
            .ToListAsync(ct);
    }


    public async Task<List<BusinessTransactionDto>> SearchIncomesAsync(
        int businessId, int userId, BusinessTransactionFilterDto f,
        CancellationToken ct = default)
    {
        var query = _context.BusinessIncomes
            .AsNoTracking()
            .Where(i => i.BusinessId == businessId && i.Business.UserId == userId);

        if (f.Year.HasValue) query = query.Where(i => i.IncomeDate.Year == f.Year.Value);
        if (f.Month.HasValue) query = query.Where(i => i.IncomeDate.Month == f.Month.Value);
        if (f.CategoryId.HasValue) query = query.Where(i => i.CategoryId == f.CategoryId.Value);
        if (f.DateFrom.HasValue) query = query.Where(i => i.IncomeDate >= f.DateFrom.Value);
        if (f.DateTo.HasValue) query = query.Where(i => i.IncomeDate <= f.DateTo.Value);
        if (f.MinAmount.HasValue) query = query.Where(i => i.Amount >= f.MinAmount.Value);
        if (f.MaxAmount.HasValue) query = query.Where(i => i.Amount <= f.MaxAmount.Value);

        if (!string.IsNullOrWhiteSpace(f.SearchText))
            query = query.Where(i =>
                (i.Source != null && i.Source.Contains(f.SearchText)) ||
                (i.Description != null && i.Description.Contains(f.SearchText)));

        return await query
            .Select(i => new BusinessTransactionDto
            {
                Id = i.IncomeId,
                Amount = i.Amount,
                Date = i.IncomeDate,
                Description = i.Description,
                Source = i.Source,
                CategoryId = i.CategoryId,
                CategoryName = i.Category.CategoryName
            })
            .OrderByDescending(i => i.Date)
            .ThenByDescending(i => i.Id)
            .ToListAsync(ct);
    }


    // ═══════════════════ العمليات — كتابة ═══════════════════

    public async Task<int> AddBusinessExpenseAsync(BusinessExpense expense)
    {
        _context.BusinessExpenses.Add(expense);
        await _context.SaveChangesAsync();
        return expense.ExpenseId;
    }


    public async Task<int> AddBusinessIncomeAsync(BusinessIncome income)
    {
        _context.BusinessIncomes.Add(income);
        await _context.SaveChangesAsync();
        return income.IncomeId;
    }


    public async Task<bool> UpdateBusinessExpenseAsync(int expenseId, int userId,
        decimal amount, DateTime date, int categoryId, byte method, string? description)
    {
        // 🔒 الفلترة بالـ userId عبر المحل
        var expense = await _context.BusinessExpenses
            .FirstOrDefaultAsync(e => e.ExpenseId == expenseId
                                   && e.Business.UserId == userId);

        if (expense is null) return false;

        expense.Amount = amount;
        expense.ExpenseDate = date.Date;
        expense.CategoryId = categoryId;
        expense.PaymentMethod = method;
        expense.Description = description;

        return await _context.SaveChangesAsync() > 0;
    }


    public async Task<bool> UpdateBusinessIncomeAsync(int incomeId, int userId,
        decimal amount, DateTime date, int categoryId, string? source, string? description)
    {
        var income = await _context.BusinessIncomes
            .FirstOrDefaultAsync(i => i.IncomeId == incomeId
                                   && i.Business.UserId == userId);

        if (income is null) return false;

        income.Amount = amount;
        income.IncomeDate = date.Date;
        income.CategoryId = categoryId;
        income.Source = source;
        income.Description = description;

        return await _context.SaveChangesAsync() > 0;
    }


    public async Task<bool> DeleteBusinessExpenseAsync(int expenseId, int userId)
    {
        var expense = await _context.BusinessExpenses
            .FirstOrDefaultAsync(e => e.ExpenseId == expenseId
                                   && e.Business.UserId == userId);

        if (expense is null) return false;

        _context.BusinessExpenses.Remove(expense);
        return await _context.SaveChangesAsync() > 0;
    }


    public async Task<bool> DeleteBusinessIncomeAsync(int incomeId, int userId)
    {
        var income = await _context.BusinessIncomes
            .FirstOrDefaultAsync(i => i.IncomeId == incomeId
                                   && i.Business.UserId == userId);

        if (income is null) return false;

        _context.BusinessIncomes.Remove(income);
        return await _context.SaveChangesAsync() > 0;
    }


    // ═══════════════════ فحوصات ═══════════════════

    public async Task<bool> BusinessBelongsToUserAsync(int businessId, int userId) =>
        await _context.Businesses
            .AsNoTracking()
            .AnyAsync(b => b.BusinessId == businessId && b.UserId == userId);


    public async Task<bool> ExpenseBelongsToUserAsync(int expenseId, int userId) =>
        await _context.BusinessExpenses
            .AsNoTracking()
            .AnyAsync(e => e.ExpenseId == expenseId && e.Business.UserId == userId);


    public async Task<bool> IncomeBelongsToUserAsync(int incomeId, int userId) =>
        await _context.BusinessIncomes
            .AsNoTracking()
            .AnyAsync(i => i.IncomeId == incomeId && i.Business.UserId == userId);


    public async Task<bool> NameExistsAsync(string name, int userId, int? excludeId = null)
    {
        var query = _context.Businesses
            .AsNoTracking()
            .Where(b => b.BusinessName == name && b.UserId == userId);

        if (excludeId.HasValue)
            query = query.Where(b => b.BusinessId != excludeId.Value);

        return await query.AnyAsync();
    }
}