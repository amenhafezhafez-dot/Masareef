namespace Masareef.Entities.DTOs;
public class UpdateDebtDto
{
    public int DebtID { get; set; }
    public string PersonName { get; set; } = null!;
    public string? PersonPhone { get; set; }
    public decimal Amount { get; set; }
    public DateTime? DueDate { get; set; }
    public string? Description { get; set; }
}