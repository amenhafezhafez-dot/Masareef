namespace Masareef.PresentationLayer;

/// <summary>Shared console helpers — avoids repeating these in every menu.</summary>
public static class ConsoleUI
{
    public static void Title(string text)
    {
        Console.Clear();
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine($"\n===== {text.ToUpper()} =====\n");
        Console.ResetColor();
    }

    public static void Section(string text) => Console.WriteLine($"\n--- {text} ---");

    public static bool TryInt(string prompt, out int value)
    {
        Console.Write(prompt);
        if (int.TryParse(Console.ReadLine(), out value)) return true;
        Error("Invalid number.");
        return false;
    }

    public static bool TryDecimal(string prompt, out decimal value)
    {
        Console.Write(prompt);
        if (decimal.TryParse(Console.ReadLine(), out value)) return true;
        Error("Invalid amount.");
        return false;
    }

    public static string Text(string prompt)
    {
        Console.Write(prompt);
        return Console.ReadLine() ?? "";
    }

    public static DateTime? OptionalDate(string prompt)
    {
        Console.Write(prompt);
        string raw = Console.ReadLine() ?? "";
        return DateTime.TryParse(raw, out var d) ? d : null;
    }

    public static bool Confirm(string prompt)
    {
        Console.Write($"{prompt} (y/n): ");
        return Console.ReadLine()?.Trim().ToLower() == "y";
    }

    public static void Result(bool ok, string message)
    {
        Console.ForegroundColor = ok ? ConsoleColor.Green : ConsoleColor.Red;
        Console.WriteLine($"\n{(ok ? "[OK]" : "[ERROR]")} {message}");
        Console.ResetColor();
        Pause();
    }

    public static void Error(string message) => Result(false, message);

    public static void Pause()
    {
        Console.WriteLine("\nPress any key...");
        Console.ReadKey(true);
    }

    /// <summary>Asks for year/month, defaults to current when left empty.</summary>
    public static (int year, int month) AskPeriod()
    {
        Console.Write($"Year [{DateTime.Today.Year}]: ");
        string y = Console.ReadLine() ?? "";
        int year = int.TryParse(y, out var yy) ? yy : DateTime.Today.Year;

        Console.Write($"Month [{DateTime.Today.Month}]: ");
        string m = Console.ReadLine() ?? "";
        int month = int.TryParse(m, out var mm) ? mm : DateTime.Today.Month;

        return (year, month);
    }
}