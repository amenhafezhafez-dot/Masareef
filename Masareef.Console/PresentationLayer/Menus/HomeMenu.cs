using Masareef.BusinessLayer.Services;
using Masareef.Entities.DTOs;
using static Masareef.PresentationLayer.ConsoleUI;

namespace Masareef.PresentationLayer.Menus;

public class HomeMenu
{
    private readonly HomeService _home;
    private readonly CategoryService _categories;
    private readonly int _userId;

    public HomeMenu(HomeService home, CategoryService categories, int userId)
    {
        _home = home;
        _categories = categories;
        _userId = userId;
    }

    public async Task ShowAsync()
    {
        while (true)
        {
            Title("Home Finance");
            Console.WriteLine("  1. Show expenses");
            Console.WriteLine("  2. Show income");
            Console.WriteLine("  3. Add expense");
            Console.WriteLine("  4. Add income");
            Console.WriteLine("  5. Delete expense");
            Console.WriteLine("  0. Back\n");
            Console.Write("Choose: ");

            switch (Console.ReadLine()?.Trim())
            {
                case "1": await ShowExpensesAsync(); break;
                case "2": await ShowIncomesAsync(); break;
                case "3": await AddExpenseAsync(); break;
                case "4": await AddIncomeAsync(); break;
                case "5": await DeleteExpenseAsync(); break;
                case "0": return;
                default: Error("Invalid choice."); break;
            }
        }
    }


    private async Task ShowExpensesAsync()
    {
        Section("Filter (Enter = current month)");
        var (year, month) = AskPeriod();

        var result = await _home.GetExpensesAsync(_userId, year, month);
        if (!result.Success) { Error(result.Message!); return; }

        var list = result.Data!;

        if (list.Count == 0) { Console.WriteLine("\n  (No expenses)"); Pause(); return; }

        Console.WriteLine($"\n{"ID",-6}{"Date",-13}{"Category",-20}{"Amount",12}  Note");
        Console.WriteLine(new string('-', 75));

        foreach (var e in list)
            Console.WriteLine($"{e.HomeExpenseId,-6}{e.ExpenseDate:yyyy-MM-dd}   " +
                              $"{Cut(e.CategoryName, 19),-20}{e.Amount,12:N2}  {e.Description}");

        Console.WriteLine(new string('-', 75));
        Console.WriteLine($"  {list.Count} items  |  Total: {list.Sum(x => x.Amount):N2}");
        Pause();
    }


    private async Task ShowIncomesAsync()
    {
        Section("Filter (Enter = current month)");
        var (year, month) = AskPeriod();

        var result = await _home.GetIncomesAsync(_userId, year, month);
        if (!result.Success) { Error(result.Message!); return; }

        var list = result.Data!;

        if (list.Count == 0) { Console.WriteLine("\n  (No income)"); Pause(); return; }

        Console.WriteLine($"\n{"ID",-6}{"Date",-13}{"Category",-20}{"Amount",12}  Source");
        Console.WriteLine(new string('-', 75));

        foreach (var i in list)
            Console.WriteLine($"{i.HomeIncomeId,-6}{i.IncomeDate:yyyy-MM-dd}   " +
                              $"{Cut(i.CategoryName, 19),-20}{i.Amount,12:N2}  {i.Source}");

        Console.WriteLine(new string('-', 75));
        Console.WriteLine($"  {list.Count} items  |  Total: {list.Sum(x => x.Amount):N2}");
        Pause();
    }


    private async Task AddExpenseAsync()
    {
        Section("Add Expense");

        int? categoryId = await PickCategoryAsync(scope: 2, kind: 1);
        if (categoryId is null) return;

        if (!TryDecimal("Amount: ", out decimal amount)) return;

        var date = OptionalDate("Date (yyyy-MM-dd) or Enter for today: ") ?? DateTime.Today;

        Console.Write("Method [1]Cash [2]Card [3]Transfer: ");
        byte.TryParse(Console.ReadLine(), out byte method);
        if (method == 0) method = 1;

        string? note = Text("Note (optional): ");

        var result = await _home.AddExpenseAsync(new CreateHomeExpenseDto
        {
            UserId = _userId,
            CategoryId = categoryId.Value,
            Amount = amount,
            ExpenseDate = date,
            PaymentMethod = method,
            Description = note
        });

        Result(result.Success, result.Success
            ? $"Expense added - ID {result.Data}" : result.Message!);
    }


    private async Task AddIncomeAsync()
    {
        Section("Add Income");

        int? categoryId = await PickCategoryAsync(scope: 2, kind: 2);
        if (categoryId is null) return;

        if (!TryDecimal("Amount: ", out decimal amount)) return;

        var date = OptionalDate("Date (yyyy-MM-dd) or Enter for today: ") ?? DateTime.Today;

        string? source = Text("Source (optional): ");
        string? note = Text("Note (optional): ");

        var result = await _home.AddIncomeAsync(new CreateHomeIncomeDto
        {
            UserId = _userId,
            CategoryId = categoryId.Value,
            Amount = amount,
            IncomeDate = date,
            Source = source,
            Description = note
        });

        Result(result.Success, result.Success
            ? $"Income added - ID {result.Data}" : result.Message!);
    }


    private async Task DeleteExpenseAsync()
    {
        Section("Delete Expense");

        if (!TryInt("Expense ID: ", out int id)) return;
        if (!Confirm("Are you sure?")) { Console.WriteLine("Cancelled."); Pause(); return; }

        var result = await _home.DeleteExpenseAsync(id, _userId);
        Result(result.Success, result.Success ? "Deleted." : result.Message!);
    }


    // ═══ shared: category picker ═══
    private async Task<int?> PickCategoryAsync(byte scope, byte kind)
    {
        var cats = await _categories.GetAsync(scope, kind, _userId);

        if (!cats.Success || cats.Data!.Count == 0)
        {
            Error("No categories available.");
            return null;
        }

        Console.WriteLine("\nCategories:");
        foreach (var c in cats.Data)
            Console.WriteLine($"  {c.CategoryId,-4} {c.CategoryName}");

        if (!TryInt("\nCategory ID: ", out int id)) return null;

        if (!cats.Data.Any(c => c.CategoryId == id))
        {
            Error("Category not in the list.");
            return null;
        }

        return id;
    }


    private static string Cut(string t, int max) =>
        t.Length <= max ? t : t[..(max - 1)] + "…";
}