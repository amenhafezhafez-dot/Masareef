using Masareef.Entities.DTOs;
using Masareef.Entities.Models;

namespace Masareef.DAL.Repository;

public interface IHomeRepository
{
    Task<List<HomeExpenseListDto>> SearchExpensesAsync(int userId, HomeFilterDto filter);

    Task<int> AddHomeExpenseAsync(HomeExpense expense);
    Task<bool> UpdateHomeExpenseAsync(int expenseId, int userId,
        decimal amount, DateTime date, int categoryId, byte method, string? description);
    Task<bool> DeleteHomeExpenseAsync(int expenseId, int userId);

    Task<List<HomeIncomeListDto>> SearchIncomesAsync(int userId, HomeFilterDto filter);

    Task<int> AddHomeIncomeAsync(HomeIncome income);
    Task<bool> UpdateHomeIncomeAsync(int incomeId, int userId,
        decimal amount, DateTime date, int categoryId, string? source, string? description);
    Task<bool> DeleteHomeIncomeAsync(int incomeId, int userId);

    Task<bool> ExpenseExistsAsync(int expenseId, int userId);
    Task<bool> IncomeExistsAsync(int incomeId, int userId);
    Task<bool> CategoryExistsAsync(int categoryId, byte kind);
}