using Masareef.BusinessLayer.Services;
using Masareef.Entities.DTOs;
using Masareef.MasareefEntities.DTOs;

namespace Masareef.PresentationLayer.Menus;

public class DebtMenu
{
    private readonly DebtService _service;
    private readonly int _userId;

    public DebtMenu(DebtService service, int userId)
    {
        _service = service;
        _userId = userId;
    }

    public async Task ShowAsync()
    {
        while (true)
        {
            Console.Clear();
            Console.WriteLine("======= DEBT MANAGEMENT =======\n");
            Console.WriteLine("  1. Show all debts");
            Console.WriteLine("  2. Open debts only");
            Console.WriteLine("  3. Overdue debts");
            Console.WriteLine("  4. Search by person name");
            Console.WriteLine("  5. Debt details");
            Console.WriteLine("  6. Add new debt");
            Console.WriteLine("  7. Record a payment");
            Console.WriteLine("  8. Write off a debt");
            Console.WriteLine("  9. Totals");
            Console.WriteLine("  0. Back\n");
            Console.Write("Choose: ");

            switch (Console.ReadLine()?.Trim())
            {
                case "1": await ShowListAsync(null); break;
                case "2": await ShowListAsync(1); break;
                case "3": await ShowOverdueAsync(); break;
                case "4": await SearchAsync(); break;
                case "5": await ShowDetailsAsync(); break;
                case "6": await CreateAsync(); break;
                case "7": await AddPaymentAsync(); break;
                case "8": await WriteOffAsync(); break;
                case "9": await ShowTotalsAsync(); break;
                case "0": return;
                default: ShowError("Invalid choice."); break;
            }
        }
    }


    // ═══════════════════ DISPLAY ═══════════════════

    private async Task ShowListAsync(byte? status)
    {
        var result = await _service.GetAllAsync(_userId, status);

        if (!result.Success) { ShowError(result.Message!); return; }

        PrintTable(result.Data!);
        Pause();
    }


    private async Task ShowOverdueAsync()
    {
        var result = await _service.GetOverdueAsync(_userId);

        if (!result.Success) { ShowError(result.Message!); return; }

        Console.WriteLine("\n*** OVERDUE DEBTS ***\n");
        PrintTable(result.Data!);
        Pause();
    }


    private async Task SearchAsync()
    {
        Console.Write("\nPerson name: ");
        string name = Console.ReadLine() ?? "";

        //var result = await _service.SearchAsync(_userId, name);

        //if (!result.Success) { ShowError(result.Message!); return; }

        //PrintTable(result.Data!);
        Pause();
    }


    private async Task ShowDetailsAsync()
    {
        if (!TryReadInt("\nDebt ID: ", out int id)) return;

        var result = await _service.GetDetailsAsync(id, _userId);

        if (!result.Success) { ShowError(result.Message!); return; }

        var d = result.Data!;

        Console.WriteLine($"\n--- {d.PersonName} ({d.DirectionText}) ---");
        Console.WriteLine($"  Phone        : {d.PersonPhone ?? "-"}");
        Console.WriteLine($"  Original     : {d.Amount,12:N2}");
        Console.WriteLine($"  Paid         : {d.TotalPaid,12:N2}");
        Console.WriteLine($"  Outstanding  : {d.Outstanding,12:N2}");
        Console.WriteLine($"  Paid %       : {d.PaidPercentage,11:N1}%");
        Console.WriteLine($"  Debt date    : {d.DebtDate:yyyy-MM-dd}");
        Console.WriteLine($"  Due date     : {d.DueDate?.ToString("yyyy-MM-dd") ?? "-"}");
        Console.WriteLine($"  Status       : {d.StatusText}{(d.IsOverdue ? "  [OVERDUE]" : "")}");

        if (d.Payments.Count > 0)
        {
            Console.WriteLine($"\n--- Payments ({d.Payments.Count}) ---");
            foreach (var p in d.Payments)
                Console.WriteLine($"  {p.PaymentDate:yyyy-MM-dd}  {p.PaidAmount,10:N2}  " +
                                  $"{p.MethodText,-10} {p.Note}");
        }
        else
        {
            Console.WriteLine("\n  (No payments yet)");
        }

        Pause();
    }


    private async Task ShowTotalsAsync()
    {
        var result = await _service.GetTotalsAsync(_userId);

        if (!result.Success) { ShowError(result.Message!); return; }

        var t = result.Data!;

        Console.WriteLine("\n--- TOTALS ---");
        Console.WriteLine($"  I owe          : {t.IOwe,14:N2}");
        Console.WriteLine($"  Owed to me     : {t.OwedToMe,14:N2}");
        Console.WriteLine($"  Net position   : {t.OwedToMe - t.IOwe,14:N2}");
        Console.WriteLine($"  Open debts     : {t.OpenCount,14}");
        Console.WriteLine($"  Overdue debts  : {t.OverdueCount,14}");

        Pause();
    }


    // ═══════════════════ INPUT ═══════════════════

    private async Task CreateAsync()
    {
        Console.WriteLine("\n--- ADD NEW DEBT ---");

        Console.Write("Person name: ");
        string name = Console.ReadLine() ?? "";

        Console.Write("Phone (optional): ");
        string? phone = Console.ReadLine();

        Console.Write("Type  [1] I owe  [2] Owed to me : ");
        if (!byte.TryParse(Console.ReadLine(), out byte direction))
        { ShowError("Invalid value."); return; }

        if (!TryReadDecimal("Amount: ", out decimal amount)) return;

        Console.Write("Due date (yyyy-MM-dd) or Enter to skip: ");
        string dueRaw = Console.ReadLine() ?? "";
        DateTime? dueDate = DateTime.TryParse(dueRaw, out var parsed) ? parsed : null;

        Console.Write("Notes (optional): ");
        string? note = Console.ReadLine();

        var result = await _service.CreateAsync(new CreateDebtDto
        {
            UserID = _userId,
            PersonName = name,
            PersonPhone = phone,
            Direction = direction,
            Amount = amount,
            DebtDate = DateTime.Today,
            DueDate = dueDate,
            Description = note
        });

        ShowResult(result.Success, result.Success
            ? $"Debt created - ID {result.Data}"
            : result.Message!);
    }


    private async Task AddPaymentAsync()
    {
        Console.WriteLine("\n--- RECORD PAYMENT ---");

        if (!TryReadInt("Debt ID: ", out int debtId)) return;

        // Show outstanding before asking for the amount
        var details = await _service.GetDetailsAsync(debtId, _userId);
        if (!details.Success) { ShowError(details.Message!); return; }

        Console.WriteLine($"  {details.Data!.PersonName} - Outstanding: {details.Data.Outstanding:N2}");

        if (!TryReadDecimal("Payment amount: ", out decimal amount)) return;

        Console.Write("Method [1]Cash [2]Card [3]Transfer: ");
        byte.TryParse(Console.ReadLine(), out byte method);
        if (method == 0) method = 1;

        Console.Write("Note (optional): ");
        string? note = Console.ReadLine();

        var result = await _service.AddPaymentAsync(new AddPaymentDto
        {
            DebtID = debtId,
            PaidAmount = amount,
            PaymentDate = DateTime.Today,
            PaymentMethod = method,
            Note = note
        }, _userId);

        ShowResult(result.Success, result.Success
            ? $"Payment recorded - ID {result.Data}"
            : result.Message!);
    }


    private async Task WriteOffAsync()
    {
        Console.WriteLine("\n--- WRITE OFF DEBT ---");

        if (!TryReadInt("Debt ID: ", out int debtId)) return;

        Console.Write("Reason: ");
        string reason = Console.ReadLine() ?? "";

        Console.Write("Are you sure? (y/n): ");
        if (Console.ReadLine()?.Trim().ToLower() != "y")
        {
            Console.WriteLine("Cancelled."); Pause(); return;
        }

        var result = await _service.WriteOffAsync(debtId, _userId, reason);

        ShowResult(result.Success, result.Success ? "Debt written off." : result.Message!);
    }


    // ═══════════════════ HELPERS ═══════════════════

    private static void PrintTable(List<DebtSummaryDto> debts)
    {
        if (debts.Count == 0)
        {
            Console.WriteLine("\n  (No debts found)");
            return;
        }

        Console.WriteLine();
        Console.WriteLine($"{"ID",-5}{"Person",-20}{"Type",-14}{"Amount",12}" +
                          $"{"Paid",12}{"Outstanding",14}  Status");
        Console.WriteLine(new string('-', 92));

        foreach (var d in debts)
        {
            Console.WriteLine(
                $"{d.DebtID,-5}{Trim(d.PersonName, 19),-20}{d.DirectionText,-14}" +
                $"{d.Amount,12:N2}{d.TotalPaid,12:N2}{d.Outstanding,14:N2}  " +
                $"{d.StatusText}{(d.IsOverdue ? " [OVERDUE]" : "")}");
        }

        Console.WriteLine(new string('-', 92));
        Console.WriteLine($"  Total: {debts.Count} debts  |  " +
                          $"Outstanding: {debts.Sum(d => d.Outstanding):N2}");
    }


    private static string Trim(string text, int max) =>
        text.Length <= max ? text : text[..(max - 1)] + "…";


    private static bool TryReadInt(string prompt, out int value)
    {
        Console.Write(prompt);
        if (int.TryParse(Console.ReadLine(), out value)) return true;

        ShowError("Invalid number.");
        return false;
    }


    private static bool TryReadDecimal(string prompt, out decimal value)
    {
        Console.Write(prompt);
        if (decimal.TryParse(Console.ReadLine(), out value)) return true;

        ShowError("Invalid amount.");
        return false;
    }


    private static void ShowResult(bool success, string message)
    {
        Console.ForegroundColor = success ? ConsoleColor.Green : ConsoleColor.Red;
        Console.WriteLine($"\n{(success ? "[OK]" : "[ERROR]")} {message}");
        Console.ResetColor();
        Pause();
    }


    private static void ShowError(string message) => ShowResult(false, message);


    private static void Pause()
    {
        Console.WriteLine("\nPress any key to continue...");
        Console.ReadKey(true);
    }
}