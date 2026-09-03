using System.Globalization;
namespace Masareef.Entities.DTOs;

/// <summary>الإنفاق حسب التصنيف — بيجاوب "فلوسي راحت فين؟"</summary>
public class CategorySpendDto
{
    public int CategoryId { get; set; }
    public string CategoryName { get; set; } = null!;
    public string? Icon { get; set; }

    public decimal Total { get; set; }
    public int Count { get; set; }
    public decimal Average { get; set; }
    public decimal Largest { get; set; }

    /// <summary>بيتحط من الـ Service — محتاج يعرف الإجمالي</summary>
    public decimal Percentage { get; set; }

    /// <summary>شريط بسيط للـ console</summary>
    public string Bar => new string('█', (int)Math.Round(Percentage / 5));
}


/// <summary>نقطة على خط الزمن — لمقارنة عدة شهور</summary>
public class MonthlyTrendDto
{
    public int Year { get; set; }
    public int Month { get; set; }
    public decimal Income { get; set; }
    public decimal Expense { get; set; }

    public decimal Net => Income - Expense;
    public string Label => $"{Month:00}/{Year}";
}


/// <summary>المصاريف الشخصية لشهر</summary>
public class HomeSpendingDto
{
    public int Year { get; set; }
    public int Month { get; set; }

    public decimal TotalSpent { get; set; }
    public decimal TotalIncome { get; set; }
    public int ExpenseCount { get; set; }

    public decimal Balance => TotalIncome - TotalSpent;

    public decimal SavingsRate =>
        TotalIncome == 0 ? 0 : Math.Round(Balance / TotalIncome * 100, 1);

    public decimal DailyAverage =>
        (Year > 0 && Month is >= 1 and <= 12)
            ? Math.Round(TotalSpent / DateTime.DaysInMonth(Year, Month), 2)
            : 0;

    public string MonthName =>
        (Year > 0 && Month is >= 1 and <= 12)
            ? new DateTime(Year, Month, 1).ToString("MMMM yyyy", new CultureInfo("ar-EG"))
            : "—";

    public string StatusText => Balance >= 0
        ? $"وفّرت {Balance:N2}"
        : $"صرفت زيادة {Math.Abs(Balance):N2}";
}


/// <summary>الشاشة الرئيسية — كل حاجة في نظرة</summary>
public class DashboardDto
{
    public int Year { get; set; }
    public int Month { get; set; }

    // المحلات
    public decimal BusinessIncome { get; set; }
    public decimal BusinessExpense { get; set; }

    // البيت
    public decimal HomeIncome { get; set; }
    public decimal HomeExpense { get; set; }

    // الديون
    public decimal IOwe { get; set; }
    public decimal OwedToMe { get; set; }
    public int OverdueCount { get; set; }

    // ---------- محسوبة ----------
    public decimal BusinessProfit => BusinessIncome - BusinessExpense;
    public decimal HomeBalance => HomeIncome - HomeExpense;
    public decimal NetDebt => OwedToMe - IOwe;

    /// <summary>⚠️ الديون مش داخلة — دي التزامات مش دخل/مصروف</summary>
    public decimal TotalNet => BusinessProfit + HomeBalance;

    public string MonthName =>
        (Year > 0 && Month is >= 1 and <= 12)
            ? new DateTime(Year, Month, 1).ToString("MMMM yyyy", new CultureInfo("ar-EG"))
            : "—";
}