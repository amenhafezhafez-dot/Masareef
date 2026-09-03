using Masareef.MasareefEntities.DTOs;

namespace Masareef.Entities.DTOs;

/// <summary>
/// Output — the debt details screen.
/// Inherits everything from Summary and adds the payment history.
/// </summary>
public class DebtDetailsDto : DebtSummaryDto
{
    public string? Description { get; set; }

    public List<PaymentDto> Payments { get; set; } = new();
}


/// <summary>One payment row inside the details screen</summary>
public class PaymentDto
{
    public int PaymentID { get; set; }
    public decimal PaidAmount { get; set; }
    public DateTime PaymentDate { get; set; }
    public byte PaymentMethod { get; set; }
    public string? Note { get; set; }

    public string MethodText => PaymentMethod switch
    {
        1 => "Cash",
        2 => "Card",
        3 => "Transfer",
        4 => "Cheque",
        _ => "Other"
    };
}