namespace Masareef.Entities.DTOs;

/// <summary>Input — إضافة مصروف للمحل</summary>
public class CreateBusinessExpenseDto
{
    public int BusinessId { get; set; }
    public int CategoryId { get; set; }
    public decimal Amount { get; set; }
    public DateTime ExpenseDate { get; set; }
    public byte PaymentMethod { get; set; } = 1;
    public string? Description { get; set; }
}

/// <summary>Input — إضافة دخل للمحل</summary>
public class CreateBusinessIncomeDto
{
    public int BusinessId { get; set; }
    public int CategoryId { get; set; }
    public decimal Amount { get; set; }
    public DateTime IncomeDate { get; set; }
    public string? Source { get; set; }
    public string? Description { get; set; }
}

/// <summary>Output — صف في قائمة عمليات المحل</summary>
public class BusinessTransactionDto
{
    public int Id { get; set; }
    public decimal Amount { get; set; }
    public DateTime Date { get; set; }
    public string? Description { get; set; }
    public string? Source { get; set; }
    public byte PaymentMethod { get; set; }
    public int CategoryId { get; set; }
    public string CategoryName { get; set; } = null!;
}