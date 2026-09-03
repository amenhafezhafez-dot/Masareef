using Microsoft.EntityFrameworkCore;
using Masareef.DAL.Context;
using Masareef.Entities.DTOs;
using Masareef.Entities.Models;

namespace Masareef.DAL.Repository;

public class HomeRepository : IHomeRepository
{
    private readonly AppDbContext _context;

    public HomeRepository(AppDbContext context) => _context = context;

    public async Task<List<HomeExpenseListDto>> SearchExpensesAsync(int userId, HomeFilterDto f)
    {
        var query = _context.HomeExpenses
            .AsNoTracking()
            .Where(e => e.UserId == userId);

        if (f.Year.HasValue)
            query = query.Where(e => e.ExpenseDate.Year == f.Year.Value);

        if (f.Month.HasValue)
            query = query.Where(e => e.ExpenseDate.Month == f.Month.Value);

        if (f.CategoryId.HasValue)
            query = query.Where(e => e.CategoryId == f.CategoryId.Value);

        if (f.DateFrom.HasValue)
            query = query.Where(e => e.ExpenseDate >= f.DateFrom.Value);

        if (f.DateTo.HasValue)
            query = query.Where(e => e.ExpenseDate <= f.DateTo.Value);

        if (f.MinAmount.HasValue)
            query = query.Where(e => e.Amount >= f.MinAmount.Value);

        if (f.MaxAmount.HasValue)
            query = query.Where(e => e.Amount <= f.MaxAmount.Value);

        if (f.PaymentMethod.HasValue)
            query = query.Where(e => e.PaymentMethod == f.PaymentMethod.Value);

        if (!string.IsNullOrWhiteSpace(f.SearchText))
            query = query.Where(e => e.Description != null
                                  && e.Description.Contains(f.SearchText));

        return await query
            .Select(e => new HomeExpenseListDto
            {
                HomeExpenseId = e.HomeExpenseId,
                Amount = e.Amount,
                ExpenseDate = e.ExpenseDate,
                PaymentMethod = e.PaymentMethod,
                Description = e.Description,
                CategoryId = e.CategoryId,
                CategoryName = e.Category.CategoryName,
                CategoryIcon = e.Category.Icon
            })
            .OrderByDescending(e => e.ExpenseDate)
            .ThenByDescending(e => e.HomeExpenseId)
            .ToListAsync();
    }


    public async Task<int> AddHomeExpenseAsync(HomeExpense expense)
    {
        _context.HomeExpenses.Add(expense);
        await _context.SaveChangesAsync();
        return expense.HomeExpenseId;
    }


    public async Task<bool> UpdateHomeExpenseAsync(
        int expenseId, int userId, decimal amount, DateTime date,
        int categoryId, byte method, string? description)
    {
        var expense = await _context.HomeExpenses
            .FirstOrDefaultAsync(e => e.HomeExpenseId == expenseId && e.UserId == userId);

        if (expense is null) return false;

        expense.Amount = amount;
        expense.ExpenseDate = date.Date;
        expense.CategoryId = categoryId;
        expense.PaymentMethod = method;
        expense.Description = description;

        return await _context.SaveChangesAsync() > 0;
    }


    public async Task<bool> DeleteHomeExpenseAsync(int expenseId, int userId)
    {
        var expense = await _context.HomeExpenses
            .FirstOrDefaultAsync(e => e.HomeExpenseId == expenseId && e.UserId == userId);

        if (expense is null) return false;

        _context.HomeExpenses.Remove(expense);
        return await _context.SaveChangesAsync() > 0;
    }


    // ═══════════════════ الدخل ═══════════════════

    public async Task<List<HomeIncomeListDto>> SearchIncomesAsync(int userId, HomeFilterDto f)
    {
        var query = _context.HomeIncomes
            .AsNoTracking()
            .Where(i => i.UserId == userId);

        if (f.Year.HasValue)
            query = query.Where(i => i.IncomeDate.Year == f.Year.Value);

        if (f.Month.HasValue)
            query = query.Where(i => i.IncomeDate.Month == f.Month.Value);

        if (f.CategoryId.HasValue)
            query = query.Where(i => i.CategoryId == f.CategoryId.Value);

        if (f.DateFrom.HasValue)
            query = query.Where(i => i.IncomeDate >= f.DateFrom.Value);

        if (f.DateTo.HasValue)
            query = query.Where(i => i.IncomeDate <= f.DateTo.Value);

        if (f.MinAmount.HasValue)
            query = query.Where(i => i.Amount >= f.MinAmount.Value);

        if (f.MaxAmount.HasValue)
            query = query.Where(i => i.Amount <= f.MaxAmount.Value);

        // للدخل بندوّر في المصدر والوصف
        if (!string.IsNullOrWhiteSpace(f.SearchText))
            query = query.Where(i =>
                (i.Source != null && i.Source.Contains(f.SearchText)) ||
                (i.Description != null && i.Description.Contains(f.SearchText)));

        return await query
            .Select(i => new HomeIncomeListDto
            {
                HomeIncomeId = i.HomeIncomeId,
                Amount = i.Amount,
                IncomeDate = i.IncomeDate,
                Source = i.Source,
                Description = i.Description,
                CategoryId = i.CategoryId,
                CategoryName = i.Category.CategoryName
            })
            .OrderByDescending(i => i.IncomeDate)
            .ThenByDescending(i => i.HomeIncomeId)
            .ToListAsync();
    }


    public async Task<int> AddHomeIncomeAsync(HomeIncome income)
    {
        _context.HomeIncomes.Add(income);
        await _context.SaveChangesAsync();
        return income.HomeIncomeId;
    }


    public async Task<bool> UpdateHomeIncomeAsync(
        int incomeId, int userId, decimal amount, DateTime date,
        int categoryId, string? source, string? description)
    {
        var income = await _context.HomeIncomes
            .FirstOrDefaultAsync(i => i.HomeIncomeId == incomeId && i.UserId == userId);

        if (income is null) return false;

        income.Amount = amount;
        income.IncomeDate = date.Date;
        income.CategoryId = categoryId;
        income.Source = source;
        income.Description = description;

        return await _context.SaveChangesAsync() > 0;
    }


    public async Task<bool> DeleteHomeIncomeAsync(int incomeId, int userId)
    {
        var income = await _context.HomeIncomes
            .FirstOrDefaultAsync(i => i.HomeIncomeId == incomeId && i.UserId == userId);

        if (income is null) return false;

        _context.HomeIncomes.Remove(income);
        return await _context.SaveChangesAsync() > 0;
    }



    /// <summary>فحص خفيف — من غير تحميل الكيان</summary>
    public async Task<bool> ExpenseExistsAsync(int expenseId, int userId) =>
        await _context.HomeExpenses
            .AsNoTracking()
            .AnyAsync(e => e.HomeExpenseId == expenseId && e.UserId == userId);

    public async Task<bool> IncomeExistsAsync(int incomeId, int userId) =>
        await _context.HomeIncomes
            .AsNoTracking()
            .AnyAsync(i => i.HomeIncomeId == incomeId && i.UserId == userId);

    public async Task<bool> CategoryExistsAsync(int categoryId, byte kind) =>
        await _context.Categories
            .AsNoTracking()
            .AnyAsync(c => c.CategoryId == categoryId
                        && c.CategoryKind == kind
                        && (c.CategoryScope == 2 || c.CategoryScope == 3));
}