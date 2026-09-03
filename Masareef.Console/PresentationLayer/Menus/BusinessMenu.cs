using Masareef.BusinessLayer.Services;
using static Masareef.PresentationLayer.ConsoleUI;

namespace Masareef.PresentationLayer.Menus;

public class BusinessMenu
{
    private readonly BusinessService _business;
    private readonly int _userId;

    public BusinessMenu(BusinessService business, int userId)
    {
        _business = business;
        _userId = userId;
    }

    public async Task ShowAsync()
    {
        while (true)
        {
            Title("Business");
            Console.WriteLine("  1. List businesses");
            Console.WriteLine("  2. Add business");
            Console.WriteLine("  3. Update business");
            Console.WriteLine("  4. Deactivate business");
            Console.WriteLine("  5. Monthly profit (one business)");
            Console.WriteLine("  6. Monthly profit (all businesses)");
            Console.WriteLine("  0. Back\n");
            Console.Write("Choose: ");

            switch (Console.ReadLine()?.Trim())
            {
                case "1": await ListAsync(); break;
                case "2": await CreateAsync(); break;
                case "3": await UpdateAsync(); break;
                case "4": await DeactivateAsync(); break;
                case "5": await ProfitOneAsync(); break;
                case "6": await ProfitAllAsync(); break;
                case "0": return;
                default: Error("Invalid choice."); break;
            }
        }
    }


    private async Task ListAsync()
    {
        var result = await _business.GetAllAsync(_userId);
        if (!result.Success) { Error(result.Message!); return; }

        var list = result.Data!;

        if (list.Count == 0) { Console.WriteLine("\n  (No businesses)"); Pause(); return; }

        Console.WriteLine($"\n{"ID",-6}{"Name",-28}{"Currency",-10}Created");
        Console.WriteLine(new string('-', 62));

        foreach (var b in list)
            Console.WriteLine($"{b.BusinessId,-6}{Cut(b.BusinessName, 27),-28}" +
                              $"{b.Currency,-10}{b.CreatedAt:yyyy-MM-dd}");

        Pause();
    }


    private async Task CreateAsync()
    {
        Section("Add Business");

        string name = Text("Business name: ");
        string? desc = Text("Description (optional): ");

        Console.Write("Currency [EGP]: ");
        string cur = Console.ReadLine() ?? "";
        if (string.IsNullOrWhiteSpace(cur)) cur = "EGP";

        var result = await _business.CreateAsync(_userId, name, desc, cur);

        Result(result.Success, result.Success
            ? $"Business created - ID {result.Data}" : result.Message!);
    }


    private async Task UpdateAsync()
    {
        Section("Update Business");

        if (!TryInt("Business ID: ", out int id)) return;

        string name = Text("New name: ");
        string? desc = Text("New description: ");

        Console.Write("Currency [EGP]: ");
        string cur = Console.ReadLine() ?? "";
        if (string.IsNullOrWhiteSpace(cur)) cur = "EGP";

        var result = await _business.UpdateAsync(id, _userId, name, desc, cur);
        Result(result.Success, result.Success ? "Updated." : result.Message!);
    }


    private async Task DeactivateAsync()
    {
        Section("Deactivate Business");

        if (!TryInt("Business ID: ", out int id)) return;

        Console.WriteLine("Note: history is kept; the business is only hidden.");
        if (!Confirm("Continue?")) { Console.WriteLine("Cancelled."); Pause(); return; }

        var result = await _business.DeactivateAsync(id, _userId);
        Result(result.Success, result.Success ? "Deactivated." : result.Message!);
    }


    private async Task ProfitOneAsync()
    {
        Section("Monthly Profit");

        if (!TryInt("Business ID: ", out int id)) return;
        var (year, month) = AskPeriod();

        var result = await _business.GetProfitAsync(id, _userId, year, month);
        if (!result.Success) { Error(result.Message!); return; }

        var p = result.Data!;

        Console.WriteLine($"\n--- {p.BusinessName} | {month:00}/{year} ---");
        Console.WriteLine($"  Income        : {p.TotalIncome,14:N2}  ({p.IncomeCount} items)");
        Console.WriteLine($"  Expense       : {p.TotalExpense,14:N2}  ({p.ExpenseCount} items)");
        Console.WriteLine(new string('-', 44));

        Console.ForegroundColor = p.NetProfit >= 0 ? ConsoleColor.Green : ConsoleColor.Red;
        Console.WriteLine($"  Net           : {p.NetProfit,14:N2}");
        Console.ResetColor();

        Console.WriteLine($"  Margin        : {p.ProfitMargin,13:N1}%");
        Pause();
    }


    private async Task ProfitAllAsync()
    {
        Section("All Businesses");
        var (year, month) = AskPeriod();

        var result = await _business.GetAllProfitsAsync(_userId, year, month);
        if (!result.Success) { Error(result.Message!); return; }

        var list = result.Data!;

        if (list.Count == 0) { Console.WriteLine("\n  (No businesses)"); Pause(); return; }

        Console.WriteLine($"\n{"Business",-26}{"Income",14}{"Expense",14}{"Net",14}");
        Console.WriteLine(new string('-', 68));

        foreach (var p in list)
        {
            Console.Write($"{Cut(p.BusinessName, 25),-26}{p.TotalIncome,14:N2}{p.TotalExpense,14:N2}");
            Console.ForegroundColor = p.NetProfit >= 0 ? ConsoleColor.Green : ConsoleColor.Red;
            Console.WriteLine($"{p.NetProfit,14:N2}");
            Console.ResetColor();
        }

        Console.WriteLine(new string('-', 68));
        Console.WriteLine($"{"TOTAL",-26}{list.Sum(x => x.TotalIncome),14:N2}" +
                          $"{list.Sum(x => x.TotalExpense),14:N2}{list.Sum(x => x.NetProfit),14:N2}");
        Pause();
    }


    private static string Cut(string t, int max) =>
        t.Length <= max ? t : t[..(max - 1)] + "…";
}