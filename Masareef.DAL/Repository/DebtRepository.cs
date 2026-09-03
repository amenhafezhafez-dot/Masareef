using Masareef.DAL.Context;
using Masareef.Entities.DTOs;
using Masareef.Entities.Models;
using Masareef.MasareefEntities.DTOs;
using Microsoft.EntityFrameworkCore;

namespace Masareef.DAL.Repository;

public class DebtRepository : IDebtRepository
{
    private readonly AppDbContext _context;
    public DebtRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<DebtSummaryDto>> SearchAsync(int userId, DebtFilterDto f)
    {
        var query = _context.Debts
            .AsNoTracking()
            .Where(d => d.UserId == userId);

        // ⭐ الفلاتر بتتبني حسب اللي المستخدم بعته — كلها في SQL
        if (f.Status.HasValue)
            query = query.Where(d => d.Status == f.Status.Value);

        if (f.Direction.HasValue)
            query = query.Where(d => d.Direction == f.Direction.Value);

        if (!string.IsNullOrWhiteSpace(f.PersonName))
            query = query.Where(d => d.PersonName.Contains(f.PersonName));

        if (f.OnlyNotDone == true)
            query = query.Where(d => d.Status == 1 || d.Status == 2);

        if (f.OnlyOverdue == true)
        {
            var today = DateTime.Today;
            query = query.Where(d => d.DueDate.HasValue
                                  && d.DueDate.Value < today
                                  && (d.Status == 1 || d.Status == 2));
        }

        if (f.DueFrom.HasValue) query = query.Where(d => d.DueDate >= f.DueFrom.Value);
        if (f.DueTo.HasValue) query = query.Where(d => d.DueDate <= f.DueTo.Value);
        if (f.DebtFrom.HasValue) query = query.Where(d => d.DebtDate >= f.DebtFrom.Value);
        if (f.DebtTo.HasValue) query = query.Where(d => d.DebtDate <= f.DebtTo.Value);

        if (f.MinAmount.HasValue) query = query.Where(d => d.Amount >= f.MinAmount.Value);
        if (f.MaxAmount.HasValue) query = query.Where(d => d.Amount <= f.MaxAmount.Value);

        return await query
            .Select(d => new DebtSummaryDto
            {
                DebtID = d.DebtId,
                PersonName = d.PersonName,
                PersonPhone = d.PersonPhone,
                Direction = d.Direction,
                Amount = d.Amount,
                TotalPaid = d.DebtPayments.Sum(p => (decimal?)p.PaidAmount) ?? 0,
                PaymentsCount = d.DebtPayments.Count(),
                DebtDate = d.DebtDate,
                DueDate = d.DueDate,
                Status = d.Status
            })
            .OrderByDescending(d => d.DebtDate)
            .ThenBy(d => d.DebtID)
            .ToListAsync();
    }
    public async Task<Debt?> GetDeptByIdAsync(int debtId, int userId) =>
        await _context.Debts
            .Include(d => d.DebtPayments)
            .FirstOrDefaultAsync(d => d.DebtId == debtId && d.UserId == userId);

    public async Task<int> AddDeptPaymentAsync(DebtPayment payment)
    {
        _context.DebtPayments.Add(payment);
        await _context.SaveChangesAsync();
        return payment.PaymentId;
    }

    public async Task<bool> UpdateDeptAsync(
        int debtId, int userId, string personName, string? personPhone,
        decimal amount, DateTime? dueDate, string? description)
    {
        var debt = await _context.Debts
        .FirstOrDefaultAsync(d => d.DebtId == debtId && d.UserId == userId);

        if (debt is null) return false;

        debt.PersonName = personName;
        debt.PersonPhone = personPhone;
        debt.Amount = amount;
        debt.DueDate = dueDate;
        debt.Description = description;

        return await _context.SaveChangesAsync() > 0;
    }


    public async Task<DebtDetailsDto?> GetDetailsAsync(int debtId, int userId) =>
    await _context.Debts
        .AsNoTracking()
        .Where(d => d.DebtId == debtId && d.UserId == userId)
        .Select(d => new DebtDetailsDto
        {
            DebtID = d.DebtId,
            PersonName = d.PersonName,
            PersonPhone = d.PersonPhone,
            Direction = d.Direction,
            Amount = d.Amount,
            TotalPaid = d.DebtPayments.Sum(p => (decimal?)p.PaidAmount) ?? 0,
            PaymentsCount = d.DebtPayments.Count(),
            DebtDate = d.DebtDate,
            DueDate = d.DueDate,
            Status = d.Status,
            Description = d.Description,
            Payments = d.DebtPayments
                .OrderByDescending(p => p.PaymentDate)
                .Select(p => new PaymentDto
                {
                    PaymentID = p.PaymentId,
                    PaidAmount = p.PaidAmount,
                    PaymentDate = p.PaymentDate,
                    PaymentMethod = p.PaymentMethod,
                    Note = p.Note
                }).ToList()
        })
        .FirstOrDefaultAsync();


    public async Task<DebtTotalsDto> GetTotalsAsync(int userId)
    {
        var rows = await _context.Debts
            .AsNoTracking()
            .Where(d => d.UserId == userId && (d.Status == 1 || d.Status == 2))
            .Select(d => new
            {
                d.Direction,
                d.DueDate,
                Outstanding = d.Amount - (d.DebtPayments.Sum(p => (decimal?)p.PaidAmount) ?? 0)
            })
            .ToListAsync();

        return new DebtTotalsDto
        {
            IOwe = rows.Where(r => r.Direction == 1).Sum(r => r.Outstanding),
            OwedToMe = rows.Where(r => r.Direction == 2).Sum(r => r.Outstanding),
            OpenCount = rows.Count,
            OverdueCount = rows.Count(r => r.DueDate.HasValue && r.DueDate.Value.Date < DateTime.Today)
        };
    }


    public async Task<int> AddDeptAsync(Debt debt)
    {
        _context.Debts.Add(debt);
        await _context.SaveChangesAsync();
        return debt.DebtId;
    }

    public async Task<int> AddPaymentAndSetStatusAsync(DebtPayment payment, byte newStatus)
    {
        _context.DebtPayments.Add(payment);

        var debt = await _context.Debts.FindAsync(payment.DebtId);
        if (debt is not null)
        {
            debt.Status = newStatus;
            _context.Entry(debt).Property(d => d.RowVersion).IsModified = true;
        }

        await _context.SaveChangesAsync();  
        return payment.PaymentId;
    }


    public async Task<bool> WriteOffAsync(int debtId, string reason)
    {
        var debt = await _context.Debts.FindAsync(debtId);
        if (debt is null) return false;

        debt.Status = 4;
        debt.WriteOffReason = reason;
        debt.WriteOffDate = DateTime.UtcNow;

        return await _context.SaveChangesAsync() > 0;
    }


    public async Task<bool> UpdateDeptStatusAsync(int userId, int debtId, byte status)
    {
        var debt = await _context.Debts
            .FirstOrDefaultAsync(d => d.DebtId == debtId && d.UserId == userId);

        if (debt is null) return false;

        debt.Status = status;
        await _context.SaveChangesAsync();
        return true;
    }


    public async Task<List<DebtSummaryDto>> GetDeptsNotDoneAsync(int userId)
    {
        return await _context.Debts
            .AsNoTracking()
            .Where(d => d.UserId == userId
                     && ( d.Status==1 || d.Status==2)
                     )   
            .Select(d => new DebtSummaryDto
            {
                DebtID = d.DebtId,
                PersonName = d.PersonName,
                PersonPhone = d.PersonPhone,
                Direction = d.Direction,
                Amount = d.Amount,
                TotalPaid = d.DebtPayments.Sum(p => (decimal?)p.PaidAmount) ?? 0,
                PaymentsCount = d.DebtPayments.Count(),
                DebtDate = d.DebtDate,
                DueDate = d.DueDate,
                Status = d.Status

            })
            .OrderBy(d => d.DueDate)
            .ToListAsync();
    }


    /// <summary>بيرجّع الحالة وإجمالي المدفوع — رقمين بس، من غير تحميل الدفعات</summary>
    public async Task<(byte Status, decimal TotalPaid)?> GetStatusAndPaidAsync(int debtId, int userId)
    {
        var row = await _context.Debts
            .AsNoTracking()
            .Where(d => d.DebtId == debtId && d.UserId == userId)
            .Select(d => new
            {
                d.Status,
                TotalPaid = d.DebtPayments.Sum(p => (decimal?)p.PaidAmount) ?? 0
            })
            .FirstOrDefaultAsync();

        return row is null ? null : (row.Status, row.TotalPaid);
    }


}