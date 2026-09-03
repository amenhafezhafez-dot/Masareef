using Masareef.DAL.Repository;
using Masareef.Entities.Common;
using Masareef.Entities.DTOs;

namespace Masareef.BusinessLayer.Services;

public class ReportService
{
    private readonly IReportRepository _repo;

    public ReportService(IReportRepository repo) => _repo = repo;


    // ═══════════════════ الشاشة الرئيسية ═══════════════════

    public async Task<Result<DashboardDto>> GetDashboardAsync(int userId, int year, int month)
    {
        if (!BusinessService.IsValidPeriod(year, month, out string error))
            return Result<DashboardDto>.Fail(error);

        return Result<DashboardDto>.Ok(
            await _repo.GetDashboardAsync(userId, year, month));
    }


    // ═══════════════════ البيت ═══════════════════

    public async Task<Result<HomeSpendingDto>> GetHomeSpendingAsync(
        int userId, int year, int month)
    {
        if (!BusinessService.IsValidPeriod(year, month, out string error))
            return Result<HomeSpendingDto>.Fail(error);

        return Result<HomeSpendingDto>.Ok(
            await _repo.GetHomeSpendingAsync(userId, year, month));
    }


    /// <summary>⭐ النسبة بتتحسب هنا — محتاجة تعرف الإجمالي</summary>
    public async Task<Result<List<CategorySpendDto>>> GetHomeBreakdownAsync(
        int userId, int year, int month)
    {
        if (!BusinessService.IsValidPeriod(year, month, out string error))
            return Result<List<CategorySpendDto>>.Fail(error);

        var list = await _repo.GetHomeCategoryBreakdownAsync(userId, year, month);

        FillPercentages(list);

        return Result<List<CategorySpendDto>>.Ok(list);
    }


    public async Task<Result<List<MonthlyTrendDto>>> GetHomeTrendAsync(
        int userId, int monthsBack = 6)
    {
        if (monthsBack is < 1 or > 24)
            return Result<List<MonthlyTrendDto>>.Fail("عدد الشهور لازم يكون بين 1 و 24.");

        return Result<List<MonthlyTrendDto>>.Ok(
            await _repo.GetHomeTrendAsync(userId, monthsBack));
    }


    // ═══════════════════ المحل ═══════════════════

    public async Task<Result<List<CategorySpendDto>>> GetBusinessBreakdownAsync(
        int businessId, int userId, int year, int month)
    {
        if (!BusinessService.IsValidPeriod(year, month, out string error))
            return Result<List<CategorySpendDto>>.Fail(error);

        var list = await _repo.GetBusinessCategoryBreakdownAsync(
            businessId, userId, year, month);

        FillPercentages(list);

        return Result<List<CategorySpendDto>>.Ok(list);
    }


    public async Task<Result<List<MonthlyTrendDto>>> GetBusinessTrendAsync(
        int businessId, int userId, int monthsBack = 6)
    {
        if (monthsBack is < 1 or > 24)
            return Result<List<MonthlyTrendDto>>.Fail("عدد الشهور لازم يكون بين 1 و 24.");

        return Result<List<MonthlyTrendDto>>.Ok(
            await _repo.GetBusinessTrendAsync(businessId, userId, monthsBack));
    }


    // ---------- مساعد ----------
    private static void FillPercentages(List<CategorySpendDto> list)
    {
        decimal total = list.Sum(x => x.Total);

        foreach (var item in list)
            item.Percentage = total == 0 ? 0 : Math.Round(item.Total / total * 100, 1);
    }
}