using static Masareef.PresentationLayer.ConsoleUI;

namespace Masareef.PresentationLayer.Menus;

public class MainMenu
{
    private readonly DebtMenu _debts;
    private readonly HomeMenu _home;
    private readonly BusinessMenu _business;
    private readonly ReportMenu _reports;
    private readonly string _userName;

    public MainMenu(DebtMenu debts, HomeMenu home,
                    BusinessMenu business, ReportMenu reports, string userName)
    {
        _debts = debts;
        _home = home;
        _business = business;
        _reports = reports;
        _userName = userName;
    }

    public async Task ShowAsync()
    {
        while (true)
        {
            Console.Clear();
            Header();

            Console.WriteLine("  1. Debts");
            Console.WriteLine("  2. Home finance");
            Console.WriteLine("  3. Business");
            Console.WriteLine("  4. Reports");
            Console.WriteLine();
            Console.WriteLine("  0. Exit\n");
            Console.Write("Choose: ");

            switch (Console.ReadLine()?.Trim())
            {
                case "1": await _debts.ShowAsync(); break;
                case "2": await _home.ShowAsync(); break;
                case "3": await _business.ShowAsync(); break;
                case "4": await _reports.ShowAsync(); break;
                case "0":
                    if (Confirm("\nExit the application?")) return;
                    break;
                default: Error("Invalid choice."); break;
            }
        }
    }

    private void Header()
    {
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("+------------------------------------------+");
        Console.WriteLine("|            M A S A R E E F               |");
        Console.WriteLine("|     Personal & Business Finance          |");
        Console.WriteLine("+------------------------------------------+");
        Console.ResetColor();
        Console.WriteLine($"  {_userName}   |   {DateTime.Now:yyyy-MM-dd}");
        Console.WriteLine(new string('-', 44) + "\n");
    }
}