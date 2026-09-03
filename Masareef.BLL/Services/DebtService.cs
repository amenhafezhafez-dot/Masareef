using Masareef.DAL.Repository;
using Masareef.Entities.Common;
using Masareef.Entities.DTOs;
using Masareef.Entities.Models;
using Masareef.MasareefEntities.DTOs;
using Microsoft.EntityFrameworkCore;

namespace Masareef.BusinessLayer.Services;

public class DebtService
{
    private readonly IDebtRepository _repo;

    public DebtService(IDebtRepository repo) => _repo = repo;


    public async Task<Result<Debt>> GetDeptByIdAsync(int debtId, int userId)
    {
        if (userId <= 0)
            return Result<Debt>.Fail("Invalid user ID.");

        if (debtId <= 0)
            return Result<Debt>.Fail("Invalid Debt ID.");

        var r = await _repo.GetDeptByIdAsync(debtId, userId);

        if (r is null)
            return Result<Debt>.Fail("Debt not found.");

        return Result<Debt>.Ok(r);
    }


    public async Task<Result<List<DebtSummaryDto>>> SearchAsync(int userId, DebtFilterDto filter)
    {
        if (userId <= 0)
            return Result<List<DebtSummaryDto>>.Fail("Invalid user ID.");

        if (filter.Status.HasValue && filter.Status is < 1 or > 4)
            return Result<List<DebtSummaryDto>>.Fail("Unknown status.");

        if (filter.MinAmount.HasValue && filter.MaxAmount.HasValue
            && filter.MinAmount > filter.MaxAmount)
            return Result<List<DebtSummaryDto>>.Fail("Min amount cannot exceed max amount.");

        if (filter.DueFrom.HasValue && filter.DueTo.HasValue
            && filter.DueFrom > filter.DueTo)
            return Result<List<DebtSummaryDto>>.Fail("Invalid date range.");

        return Result<List<DebtSummaryDto>>.Ok(await _repo.SearchAsync(userId, filter));
    }

    public Task<Result<List<DebtSummaryDto>>> GetAllAsync(int userId, byte? status = null) =>
        SearchAsync(userId, new DebtFilterDto { Status = status });

    public Task<Result<List<DebtSummaryDto>>> GetOverdueAsync(int userId) =>
        SearchAsync(userId, new DebtFilterDto { OnlyOverdue = true });

    public Task<Result<List<DebtSummaryDto>>> GetNotDoneAsync(int userId) =>
        SearchAsync(userId, new DebtFilterDto { OnlyNotDone = true });

    public Task<Result<List<DebtSummaryDto>>> SearchByNameAsync(int userId, string name) =>
        string.IsNullOrWhiteSpace(name) || name.Trim().Length < 2
            ? Task.FromResult(Result<List<DebtSummaryDto>>.Fail("Name must be at least 2 characters."))
            : SearchAsync(userId, new DebtFilterDto { PersonName = name.Trim() });

    public async Task<Result<DebtDetailsDto>> GetDetailsAsync(int debtId, int userId)
    {
        if (debtId <= 0)
            return Result<DebtDetailsDto>.Fail("Invalid debt ID.");

        if (userId <= 0)
            return Result<DebtDetailsDto>.Fail("Invalid User ID.");

        var dto = await _repo.GetDetailsAsync(debtId, userId);

        return dto is null
            ? Result<DebtDetailsDto>.Fail("Debt not found.")
            : Result<DebtDetailsDto>.Ok(dto);
    }

    public async Task<Result<DebtTotalsDto>> GetTotalsAsync(int userId)
    {
        if (userId <= 0)
            return Result<DebtTotalsDto>.Fail("Invalid user ID.");

        var dto = await _repo.GetTotalsAsync(userId);

        return dto is null
            ? Result<DebtTotalsDto>.Fail("All Debt not found.")
            : Result<DebtTotalsDto>.Ok(dto);
    }





    public async Task<Result<int>> CreateAsync(CreateDebtDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.PersonName))
            return Result<int>.Fail("Person name is required.");

        if (dto.PersonName.Trim().Length > 100)
            return Result<int>.Fail("Person name is too long (max 100).");

        if (dto.Amount <= 0)
            return Result<int>.Fail("Amount must be greater than zero.");

        if (dto.Amount > 100_000_000)
            return Result<int>.Fail("Amount is unrealistically large.");

        if (dto.Direction is not (1 or 2))
            return Result<int>.Fail("Direction must be 1 (I owe) or 2 (Owed to me).");

        if (dto.DebtDate.Date > DateTime.Today)
            return Result<int>.Fail("Debt date cannot be in the future.");

        if (dto.DueDate.HasValue && dto.DueDate.Value.Date < dto.DebtDate.Date)
            return Result<int>.Fail("Due date cannot be before the debt date.");

        var debt = new Debt
        {
            UserId = dto.UserID,
            BusinessId = dto.BusinessID,
            PersonName = dto.PersonName.Trim(),
            PersonPhone = string.IsNullOrWhiteSpace(dto.PersonPhone)
                          ? null : dto.PersonPhone.Trim(),
            Direction = dto.Direction,
            Amount = dto.Amount,
            DebtDate = dto.DebtDate.Date,
            DueDate = dto.DueDate?.Date,
            Description = dto.Description?.Trim(),
            Status = 1,
            CreatedAt = DateTime.UtcNow
        };

        return Result<int>.Ok(await _repo.AddDeptAsync(debt));
    }

    public async Task<Result<int>> AddPaymentAsync(AddPaymentDto dto, int userId)
    {
        var debt = await _repo.GetDeptByIdAsync(dto.DebtID, userId);

        if (debt is null)
            return Result<int>.Fail("Debt not found.");

        if (debt.Status == 3)
            return Result<int>.Fail("This debt is already fully settled.");

        if (debt.Status == 4)
            return Result<int>.Fail("This debt is written off; payments are not allowed.");

        if (dto.PaidAmount <= 0)
            return Result<int>.Fail("Payment must be greater than zero.");

        decimal totalPaid = debt.DebtPayments.Sum(p => p.PaidAmount);
        decimal outstanding = debt.Amount - totalPaid;

        if (dto.PaidAmount > outstanding)
            return Result<int>.Fail(
                $"Payment ({dto.PaidAmount:N2}) exceeds outstanding ({outstanding:N2}).");

        if (dto.PaymentDate.Date > DateTime.Today)
            return Result<int>.Fail("Payment date cannot be in the future.");

        if (dto.PaymentDate.Date < debt.DebtDate.Date)
            return Result<int>.Fail("Payment date cannot be before the debt date.");

        var payment = new DebtPayment
        {
            DebtId = dto.DebtID,
            PaidAmount = dto.PaidAmount,
            PaymentDate = dto.PaymentDate.Date,
            PaymentMethod = dto.PaymentMethod,
            Note = dto.Note?.Trim(),
            CreatedAt = DateTime.UtcNow
        };

        decimal newTotal = totalPaid + dto.PaidAmount;
        byte newStatus = newTotal >= debt.Amount ? (byte)3 : (byte)2;

        try
        {
            int paymentId = await _repo.AddPaymentAndSetStatusAsync(payment, newStatus);
            return Result<int>.Ok(paymentId);
        }
        catch (DbUpdateConcurrencyException)
        {
            return Result<int>.Fail("تم تعديل الدين من جهة أخرى، حاول مرة أخرى.");
        }
    }


    public async Task<Result> UpdateDeptAsync(
        int debtId, int userId, string personName, string? personPhone,
        decimal amount, DateTime? dueDate, string? description)
    {
        if (debtId <= 0 || userId <= 0)
            return Result.Fail("Invalid parameters.");

        if (string.IsNullOrWhiteSpace(personName))
            return Result.Fail("Person name is required.");

        if (personName.Trim().Length > 100)
            return Result.Fail("Person name is too long (max 100).");

        if (amount <= 0)
            return Result.Fail("Amount must be greater than zero.");

        if (amount > 900_000_000)
            return Result.Fail("Amount is unrealistically large.");


        var info = await _repo.GetStatusAndPaidAsync(debtId, userId);

        if (info is null)
            return Result.Fail("Debt not found.");

        var (status, totalPaid) = info.Value;

        if (status == 3) return Result.Fail("A settled debt cannot be edited.");
        if (status == 4) return Result.Fail("A written-off debt cannot be edited.");

        if (amount < totalPaid)
            return Result.Fail(
                $"New amount ({amount:N2}) is less than already paid ({totalPaid:N2}).");

        bool updated = await _repo.UpdateDeptAsync(
            debtId, userId, personName.Trim(), personPhone?.Trim(),
            amount, dueDate?.Date, description?.Trim());

        if (!updated) return Result.Fail("Failed to update debt.");


        byte newStatus = totalPaid == 0 ? (byte)1
                       : totalPaid >= amount ? (byte)3
                                             : (byte)2;

        if (newStatus != status)
            await _repo.UpdateDeptStatusAsync(userId, debtId, newStatus);

        return Result.Ok();
    }

    public async Task<Result> WriteOffAsync(int debtId, int userId, string reason)
    {
        var debt = await _repo.GetDeptByIdAsync(debtId, userId);

        if (debt is null)
            return Result.Fail("Debt not found.");

        if (debt.Status == 3)
            return Result.Fail("A settled debt cannot be written off.");

        if (debt.Status == 4)
            return Result.Fail("This debt is already written off.");

        if (string.IsNullOrWhiteSpace(reason))
            return Result.Fail("A reason is required.");

        bool ok = await _repo.WriteOffAsync(debtId, reason.Trim());

        return ok ? Result.Ok() : Result.Fail("Write-off failed.");
    }

}