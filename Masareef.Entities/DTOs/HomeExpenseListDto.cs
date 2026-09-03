using System.Globalization;

namespace Masareef.Entities.DTOs;

public class HomeExpenseListDto
{
    public int HomeExpenseId { get; set; }
    public decimal Amount { get; set; }
    public DateTime ExpenseDate { get; set; }
    public byte PaymentMethod { get; set; }
    public string? Description { get; set; }

    public int CategoryId { get; set; }
    public string CategoryName { get; set; } = null!;
    public string? CategoryIcon { get; set; }

    public string MethodText => PaymentMethod switch
    {
        1 => "كاش",
        2 => "كارت",
        3 => "تحويل",
        _ => "أخرى"
    };

    public string DateText =>
        ExpenseDate.ToString("dd MMM yyyy", new CultureInfo("ar-EG"));
}

/// <summary>Output — صف في قائمة الدخل الشخصي</summary>
public class HomeIncomeListDto
{
    public int HomeIncomeId { get; set; }
    public decimal Amount { get; set; }
    public DateTime IncomeDate { get; set; }
    public string? Source { get; set; }
    public string? Description { get; set; }

    public int CategoryId { get; set; }
    public string CategoryName { get; set; } = null!;

    public string DateText =>
        IncomeDate.ToString("dd MMM yyyy", new CultureInfo("ar-EG"));
}

/// <summary>Input — إضافة مصروف</summary>
public class CreateHomeExpenseDto
{
    public int UserId { get; set; }
    public int CategoryId { get; set; }
    public decimal Amount { get; set; }
    public DateTime ExpenseDate { get; set; }
    public byte PaymentMethod { get; set; } = 1;
    public string? Description { get; set; }
}

/// <summary>Input — إضافة دخل</summary>
public class CreateHomeIncomeDto
{
    public int UserId { get; set; }
    public int CategoryId { get; set; }
    public decimal Amount { get; set; }
    public DateTime IncomeDate { get; set; }
    public string? Source { get; set; }
    public string? Description { get; set; }
}