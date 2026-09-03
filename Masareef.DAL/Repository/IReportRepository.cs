using Masareef.Entities.DTOs;

namespace Masareef.DAL.Repository;

public interface IReportRepository
{
    // ═══ المحل ═══
    Task<BusinessProfitDto?> GetBusinessProfitAsync(int businessId, int userId, int year, int month);
    Task<List<BusinessProfitDto>> GetAllBusinessProfitsAsync(int userId, int year, int month);
    Task<List<MonthlyTrendDto>> GetBusinessTrendAsync(int businessId, int userId, int monthsBack);
    Task<List<CategorySpendDto>> GetBusinessCategoryBreakdownAsync(int businessId, int userId, int year, int month);

    // ═══ البيت ═══
    Task<HomeSpendingDto> GetHomeSpendingAsync(int userId, int year, int month);
    Task<List<MonthlyTrendDto>> GetHomeTrendAsync(int userId, int monthsBack);
    Task<List<CategorySpendDto>> GetHomeCategoryBreakdownAsync(int userId, int year, int month);

    // ═══ الشاشة الرئيسية ═══
    Task<DashboardDto> GetDashboardAsync(int userId, int year, int month);
}