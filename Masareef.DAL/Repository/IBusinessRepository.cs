using Masareef.Entities.DTOs;
using Masareef.Entities.Models;

namespace Masareef.DAL.Repository;

public interface IBusinessRepository
{
    // ═══════════ المحلات — قراءة ═══════════
    /// <summary>⭐ البحث الأساسي في المحلات</summary>
    Task<List<BusinessListDto>> SearchAsync(int userId, BusinessFilterDto filter,
        CancellationToken ct = default);

    Task<BusinessListDto?> GetDetailsAsync(int businessId, int userId, CancellationToken ct = default);      // للقراءة
    Task<Business?> GetEntityForUpdateAsync(int businessId, int userId);     // للتعديل
    // ═══════════ المحلات — أرباح ═══════════
    Task<BusinessProfitDto?> GetProfitForOneBusinessInMounthAsync(
        int businessId, int userId, int year, int month, CancellationToken ct = default);

    Task<List<BusinessProfitDto>> GetProfitsForAllBussinessAsync(
        int userId, int year, int month, CancellationToken ct = default);

    Task<BusinessProfitDto?> GetProfitForBusinessEverAsync(int businessId, int userId);

    // ═══════════ المحلات — كتابة ═══════════
    Task<int> AddBusinessAsync(Business business);
    Task<bool> UpdateBusinessAsync(int businessId, int userId,
        string name, string? description, string currency);
    Task<bool> DeactivateBusinessAsync(int businessId, int userId);
    Task<bool> ReactivateBusinessAsync(int businessId, int userId);

    // ═══════════ العمليات — قراءة ═══════════
    Task<List<BusinessTransactionDto>> SearchExpensesAsync(
        int businessId, int userId, BusinessTransactionFilterDto filter,
        CancellationToken ct = default);

    Task<List<BusinessTransactionDto>> SearchIncomesAsync(
        int businessId, int userId, BusinessTransactionFilterDto filter,
        CancellationToken ct = default);

    // ═══════════ العمليات — كتابة ═══════════
    Task<int> AddBusinessExpenseAsync(BusinessExpense expense);
    Task<int> AddBusinessIncomeAsync(BusinessIncome income);

    Task<bool> UpdateBusinessExpenseAsync(int expenseId, int userId,
        decimal amount, DateTime date, int categoryId, byte method, string? description);

    Task<bool> UpdateBusinessIncomeAsync(int incomeId, int userId,
        decimal amount, DateTime date, int categoryId, string? source, string? description);

    Task<bool> DeleteBusinessExpenseAsync(int expenseId, int userId);
    Task<bool> DeleteBusinessIncomeAsync(int incomeId, int userId);

    // ═══════════ فحوصات ═══════════
    Task<bool> BusinessBelongsToUserAsync(int businessId, int userId);
    Task<bool> ExpenseBelongsToUserAsync(int expenseId, int userId);
    Task<bool> IncomeBelongsToUserAsync(int incomeId, int userId);
    Task<bool> NameExistsAsync(string name, int userId, int? excludeId = null);
}