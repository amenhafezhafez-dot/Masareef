using System.Globalization;

namespace Masareef.Entities.DTOs;
public class BusinessProfitDto
{
    public int BusinessId { get; set; }
    public string BusinessName { get; set; } = null!;
    public int Year { get; set; }
    public int Month { get; set; }

    public DateTime createdAt { get; set; } 

    // ---------- من SQL ----------
    public decimal TotalIncome { get; set; }
    public decimal TotalExpense { get; set; }
    public int IncomeCount { get; set; }
    public int ExpenseCount { get; set; }

    public decimal NetProfit => TotalIncome - TotalExpense;

    public bool IsProfitable => NetProfit > 0;

    public decimal ProfitMargin =>
        TotalIncome == 0 ? 0 : Math.Round(NetProfit / TotalIncome * 100, 1);

    public decimal AverageIncome =>
        IncomeCount == 0 ? 0 : Math.Round(TotalIncome / IncomeCount, 2);

    public decimal AverageExpense =>
        ExpenseCount == 0 ? 0 : Math.Round(TotalExpense / ExpenseCount, 2);

    public string MonthName =>
        (Year > 0 && Month is >= 1 and <= 12)
            ? new DateTime(Year, Month, 1).ToString("MMMM yyyy", new CultureInfo("ar-EG"))
            : "—";

    public string ResultText => NetProfit switch
    {
        > 0 => $"Profit {NetProfit:N2}",
        < 0 => $"loss {Math.Abs(NetProfit):N2}",
        _ => "nothing"
    };
}