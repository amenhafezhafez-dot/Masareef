using Masareef.BusinessLayer.Services;
using static Masareef.PresentationLayer.ConsoleUI;

namespace Masareef.PresentationLayer.Menus;

public class ReportMenu
{
    private readonly ReportService _reports;
    private readonly int _userId;

    public ReportMenu(ReportService reports, int userId)
    {
        _reports = reports;
        _userId = userId;
    }

    public async Task ShowAsync()
    {
        while (true)
        {
            Title("Reports");
            Console.WriteLine("  1. Dashboard (this month)");
            Console.WriteLine("  2. Home spending summary");
            Console.WriteLine("  3. Home spending by category");
            Console.WriteLine("  4. Home trend (last 6 months)");
            Console.WriteLine("  0. Back\n");
            Console.Write("Choose: ");

            switch (Console.ReadLine()?.Trim())
            {
                case "1": await DashboardAsync(); break;
                case "2": await HomeSummaryAsync(); break;
                case "3": await BreakdownAsync(); break;
                case "4": await TrendAsync(); break;
                case "0": return;
                default: Error("Invalid choice."); break;
            }
        }
    }


    private async Task DashboardAsync()
    {
        var (year, month) = (DateTime.Today.Year, DateTime.Today.Month);

        var result = await _reports.GetDashboardAsync(_userId, year, month);
        if (!result.Success) { Error(result.Message!); return; }

        var d = result.Data!;

        Console.WriteLine($"\n===== DASHBOARD  {month:00}/{year} =====\n");

        Console.WriteLine("  BUSINESS");
        Console.WriteLine($"    Income        : {d.BusinessIncome,14:N2}");
        Console.WriteLine($"    Expense       : {d.BusinessExpense,14:N2}");
        PrintSigned("    Profit        : ", d.BusinessProfit);

        Console.WriteLine("\n  HOME");
        Console.WriteLine($"    Income        : {d.HomeIncome,14:N2}");
        Console.WriteLine($"    Expense       : {d.HomeExpense,14:N2}");
        PrintSigned("    Balance       : ", d.HomeBalance);

        Console.WriteLine("\n  DEBTS  (not part of profit)");
        Console.WriteLine($"    I owe         : {d.IOwe,14:N2}");
        Console.WriteLine($"    Owed to me    : {d.OwedToMe,14:N2}");
        PrintSigned("    Net debt      : ", d.NetDebt);

        if (d.OverdueCount > 0)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"    [!] {d.OverdueCount} overdue debt(s)");
            Console.ResetColor();
        }

        Console.WriteLine("\n" + new string('=', 40));
        PrintSigned("  TOTAL NET     : ", d.TotalNet);

        Pause();
    }


    private async Task HomeSummaryAsync()
    {
        var (year, month) = AskPeriod();

        var result = await _reports.GetHomeSpendingAsync(_userId, year, month);
        if (!result.Success) { Error(result.Message!); return; }

        var s = result.Data!;

        Console.WriteLine($"\n--- Home  {month:00}/{year} ---");
        Console.WriteLine($"  Income        : {s.TotalIncome,14:N2}");
        Console.WriteLine($"  Spent         : {s.TotalSpent,14:N2}  ({s.ExpenseCount} items)");
        PrintSigned("  Balance       : ", s.Balance);
        Console.WriteLine($"  Savings rate  : {s.SavingsRate,13:N1}%");
        Console.WriteLine($"  Daily average : {s.DailyAverage,14:N2}");

        Pause();
    }


    private async Task BreakdownAsync()
    {
        var (year, month) = AskPeriod();

        var result = await _reports.GetHomeBreakdownAsync(_userId, year, month);
        if (!result.Success) { Error(result.Message!); return; }

        var list = result.Data!;

        if (list.Count == 0) { Console.WriteLine("\n  (No data)"); Pause(); return; }

        Console.WriteLine($"\n--- Where the money went  {month:00}/{year} ---\n");
        Console.WriteLine($"{"Category",-22}{"Total",12}{"%",8}  Chart");
        Console.WriteLine(new string('-', 70));

        foreach (var c in list)
            Console.WriteLine($"{Cut(c.CategoryName, 21),-22}{c.Total,12:N2}" +
                              $"{c.Percentage,7:N1}%  {c.Bar}");

        Console.WriteLine(new string('-', 70));
        Console.WriteLine($"{"TOTAL",-22}{list.Sum(x => x.Total),12:N2}");
        Pause();
    }


    private async Task TrendAsync()
    {
        var result = await _reports.GetHomeTrendAsync(_userId, 6);
        if (!result.Success) { Error(result.Message!); return; }

        Console.WriteLine("\n--- Last 6 months ---\n");
        Console.WriteLine($"{"Month",-10}{"Income",14}{"Expense",14}{"Net",14}");
        Console.WriteLine(new string('-', 52));

        foreach (var m in result.Data!)
        {
            Console.Write($"{m.Label,-10}{m.Income,14:N2}{m.Expense,14:N2}");
            Console.ForegroundColor = m.Net >= 0 ? ConsoleColor.Green : ConsoleColor.Red;
            Console.WriteLine($"{m.Net,14:N2}");
            Console.ResetColor();
        }

        Pause();
    }


    private static void PrintSigned(string label, decimal value)
    {
        Console.Write(label);
        Console.ForegroundColor = value >= 0 ? ConsoleColor.Green : ConsoleColor.Red;
        Console.WriteLine($"{value,14:N2}");
        Console.ResetColor();
    }

    private static string Cut(string t, int max) =>
        t.Length <= max ? t : t[..(max - 1)] + "…";
}