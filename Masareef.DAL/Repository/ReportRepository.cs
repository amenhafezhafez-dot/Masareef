using Microsoft.EntityFrameworkCore;
using Masareef.DAL.Context;
using Masareef.Entities.DTOs;

namespace Masareef.DAL.Repository;

public class ReportRepository : IReportRepository
{
    private readonly AppDbContext _context;

    public ReportRepository(AppDbContext context) => _context = context;


    ///═══════════════════ المحل ═══════════════════

    public async Task<BusinessProfitDto?> GetBusinessProfitAsync(
        int businessId, int userId, int year, int month)
    {
        return await _context.Businesses
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
            .FirstOrDefaultAsync();
    }


    public async Task<List<BusinessProfitDto>> GetAllBusinessProfitsAsync(
        int userId, int year, int month)
    {
        return await _context.Businesses
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
                    .Sum(e => (decimal?)e.Amount) ?? 0
            })
            .OrderByDescending(b => b.TotalIncome - b.TotalExpense)
            .ToListAsync();
    }


    /// <summary>اتجاه آخر N شهر</summary>
    public async Task<List<MonthlyTrendDto>> GetBusinessTrendAsync(
        int businessId, int userId, int monthsBack)
    {
        var from = DateTime.Today.AddMonths(-monthsBack + 1);
        from = new DateTime(from.Year, from.Month, 1);

        // ⚠️ استعلامين — GroupBy على جدولين مختلفين مينفعش يتدمج في واحد
        var incomes = await _context.BusinessIncomes
            .AsNoTracking()
            .Where(i => i.BusinessId == businessId
                     && i.Business.UserId == userId
                     && i.IncomeDate >= from)
            .GroupBy(i => new { i.IncomeDate.Year, i.IncomeDate.Month })
            .Select(g => new { g.Key.Year, g.Key.Month, Total = g.Sum(x => x.Amount) })
            .ToListAsync();

        var expenses = await _context.BusinessExpenses
            .AsNoTracking()
            .Where(e => e.BusinessId == businessId
                     && e.Business.UserId == userId
                     && e.ExpenseDate >= from)
            .GroupBy(e => new { e.ExpenseDate.Year, e.ExpenseDate.Month })
            .Select(g => new { g.Key.Year, g.Key.Month, Total = g.Sum(x => x.Amount) })
            .ToListAsync();

        // بناء كل الشهور — حتى الفاضية
        var result = new List<MonthlyTrendDto>();

        for (int i = 0; i < monthsBack; i++)
        {
            var d = from.AddMonths(i);

            result.Add(new MonthlyTrendDto
            {
                Year = d.Year,
                Month = d.Month,
                Income = incomes.FirstOrDefault(x => x.Year == d.Year && x.Month == d.Month)?.Total ?? 0,
                Expense = expenses.FirstOrDefault(x => x.Year == d.Year && x.Month == d.Month)?.Total ?? 0
            });
        }

        return result;
    }


    public async Task<List<CategorySpendDto>> GetBusinessCategoryBreakdownAsync(
        int businessId, int userId, int year, int month)
    {
        return await _context.BusinessExpenses
            .AsNoTracking()
            .Where(e => e.BusinessId == businessId
                     && e.Business.UserId == userId
                     && e.ExpenseDate.Year == year
                     && e.ExpenseDate.Month == month)
            .GroupBy(e => new { e.CategoryId, e.Category.CategoryName, e.Category.Icon })
            .Select(g => new CategorySpendDto
            {
                CategoryId = g.Key.CategoryId,
                CategoryName = g.Key.CategoryName,
                Icon = g.Key.Icon,
                Total = g.Sum(e => e.Amount),
                Count = g.Count(),
                Average = g.Average(e => e.Amount),
                Largest = g.Max(e => e.Amount)
            })
            .OrderByDescending(x => x.Total)
            .ToListAsync();
    }


    // ═══════════════════ البيت ═══════════════════

    public async Task<HomeSpendingDto> GetHomeSpendingAsync(int userId, int year, int month)
    {
        // ⭐ GroupBy(x => 1) — كل الأرقام في round trip واحدة
        var spending = await _context.HomeExpenses
            .AsNoTracking()
            .Where(e => e.UserId == userId
                     && e.ExpenseDate.Year == year
                     && e.ExpenseDate.Month == month)
            .GroupBy(e => 1)
            .Select(g => new { Total = g.Sum(e => e.Amount), Count = g.Count() })
            .FirstOrDefaultAsync();

        var income = await _context.HomeIncomes
            .AsNoTracking()
            .Where(i => i.UserId == userId
                     && i.IncomeDate.Year == year
                     && i.IncomeDate.Month == month)
            .SumAsync(i => (decimal?)i.Amount) ?? 0;

        return new HomeSpendingDto
        {
            Year = year,
            Month = month,
            TotalSpent = spending?.Total ?? 0,
            ExpenseCount = spending?.Count ?? 0,
            TotalIncome = income
        };
    }


    public async Task<List<MonthlyTrendDto>> GetHomeTrendAsync(int userId, int monthsBack)
    {
        var from = DateTime.Today.AddMonths(-monthsBack + 1);
        from = new DateTime(from.Year, from.Month, 1);

        var incomes = await _context.HomeIncomes
            .AsNoTracking()
            .Where(i => i.UserId == userId && i.IncomeDate >= from)
            .GroupBy(i => new { i.IncomeDate.Year, i.IncomeDate.Month })
            .Select(g => new { g.Key.Year, g.Key.Month, Total = g.Sum(x => x.Amount) })
            .ToListAsync();

        var expenses = await _context.HomeExpenses
            .AsNoTracking()
            .Where(e => e.UserId == userId && e.ExpenseDate >= from)
            .GroupBy(e => new { e.ExpenseDate.Year, e.ExpenseDate.Month })
            .Select(g => new { g.Key.Year, g.Key.Month, Total = g.Sum(x => x.Amount) })
            .ToListAsync();

        var result = new List<MonthlyTrendDto>();

        for (int i = 0; i < monthsBack; i++)
        {
            var d = from.AddMonths(i);

            result.Add(new MonthlyTrendDto
            {
                Year = d.Year,
                Month = d.Month,
                Income = incomes.FirstOrDefault(x => x.Year == d.Year && x.Month == d.Month)?.Total ?? 0,
                Expense = expenses.FirstOrDefault(x => x.Year == d.Year && x.Month == d.Month)?.Total ?? 0
            });
        }

        return result;
    }


    public async Task<List<CategorySpendDto>> GetHomeCategoryBreakdownAsync(
        int userId, int year, int month)
    {
        return await _context.HomeExpenses
            .AsNoTracking()
            .Where(e => e.UserId == userId
                     && e.ExpenseDate.Year == year
                     && e.ExpenseDate.Month == month)
            .GroupBy(e => new { e.CategoryId, e.Category.CategoryName, e.Category.Icon })
            .Select(g => new CategorySpendDto
            {
                CategoryId = g.Key.CategoryId,
                CategoryName = g.Key.CategoryName,
                Icon = g.Key.Icon,
                Total = g.Sum(e => e.Amount),
                Count = g.Count(),
                Average = g.Average(e => e.Amount),
                Largest = g.Max(e => e.Amount)
            })
            .OrderByDescending(x => x.Total)
            .ToListAsync();
    }


    // ═══════════════════ الشاشة الرئيسية ═══════════════════

    public async Task<DashboardDto> GetDashboardAsync(int userId, int year, int month)
    {
        var busIncome = await _context.BusinessIncomes
            .AsNoTracking()
            .Where(i => i.Business.UserId == userId
                     && i.IncomeDate.Year == year && i.IncomeDate.Month == month)
            .SumAsync(i => (decimal?)i.Amount) ?? 0;

        var busExpense = await _context.BusinessExpenses
            .AsNoTracking()
            .Where(e => e.Business.UserId == userId
                     && e.ExpenseDate.Year == year && e.ExpenseDate.Month == month)
            .SumAsync(e => (decimal?)e.Amount) ?? 0;

        var homeIncome = await _context.HomeIncomes
            .AsNoTracking()
            .Where(i => i.UserId == userId
                     && i.IncomeDate.Year == year && i.IncomeDate.Month == month)
            .SumAsync(i => (decimal?)i.Amount) ?? 0;

        var homeExpense = await _context.HomeExpenses
            .AsNoTracking()
            .Where(e => e.UserId == userId
                     && e.ExpenseDate.Year == year && e.ExpenseDate.Month == month)
            .SumAsync(e => (decimal?)e.Amount) ?? 0;

        // الديون — الحالة المفتوحة والجزئية بس
        var debts = await _context.Debts
            .AsNoTracking()
            .Where(d => d.UserId == userId && (d.Status == 1 || d.Status == 2))
            .Select(d => new
            {
                d.Direction,
                d.DueDate,
                Outstanding = d.Amount - (d.DebtPayments.Sum(p => (decimal?)p.PaidAmount) ?? 0)
            })
            .ToListAsync();

        return new DashboardDto
        {
            Year = year,
            Month = month,
            BusinessIncome = busIncome,
            BusinessExpense = busExpense,
            HomeIncome = homeIncome,
            HomeExpense = homeExpense,
            IOwe = debts.Where(d => d.Direction == 1).Sum(d => d.Outstanding),
            OwedToMe = debts.Where(d => d.Direction == 2).Sum(d => d.Outstanding),
            OverdueCount = debts.Count(d => d.DueDate.HasValue
                                            && d.DueDate.Value.Date < DateTime.Today)
        };
    }
}