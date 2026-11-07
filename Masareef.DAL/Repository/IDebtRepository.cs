using Masareef.Entities.DTOs;
using Masareef.Entities.Models;
using Masareef.MasareefEntities.DTOs;

namespace Masareef.DAL.Repository;

public interface IDebtRepository
{
    Task<List<DebtSummaryDto>> SearchAsync(int userId, DebtFilterDto filter);

    Task<Debt?> GetDeptByIdAsync(int debtId, int userId);

    /// <summary>
    /// بيجيب الدين بالـ ID بس من غير فلترة بالمالك — مخصص للحالات اللي الـ
    /// OwnershipRequirementHandler هو اللي بيتحقق من الملكية فيها. متستخدمهاش في أي حتة تانية.
    /// </summary>
    Task<Debt?> GetByIdForOwnershipCheckAsync(int debtId);
    Task<DebtDetailsDto?> GetDetailsAsync(int debtId, int userId);
    Task<DebtTotalsDto> GetTotalsAsync(int userId);

    Task<int> AddDeptAsync(Debt debt);
    Task<int> AddPaymentAndSetStatusAsync(DebtPayment payment, byte newStatus);
    Task<bool> UpdateDeptAsync(int debtId, int userId, string personName,
        string? personPhone, decimal amount, DateTime? dueDate, string? description);
    Task<bool> UpdateDeptStatusAsync( int userId ,int debtId, byte status);
    Task<bool> WriteOffAsync(int debtId, string reason);

    Task<(byte Status, decimal TotalPaid)?> GetStatusAndPaidAsync(int debtId, int userId);
}