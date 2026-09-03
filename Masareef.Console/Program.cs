using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Masareef.DAL.Context;
using Masareef.DAL.Repository;
using Masareef.BusinessLayer.Services;
using Masareef.PresentationLayer.Menus;

Console.OutputEncoding = System.Text.Encoding.UTF8;

// ---------- config ----------
IConfiguration config = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: false)
    .Build();

string? cs = config.GetConnectionString("DefaultConnection");

if (string.IsNullOrWhiteSpace(cs))
{
    Console.WriteLine("[ERROR] Connection string not found.");
    Console.ReadKey(true);
    return;
}

// ---------- context ----------
var options = new DbContextOptionsBuilder<AppDbContext>()
    .UseSqlServer(cs)
    // .LogTo(Console.WriteLine, LogLevel.Information)   ← enable to inspect SQL
    .Options;

using var context = new AppDbContext(options);

if (!context.Database.CanConnect())
{
    Console.WriteLine("[ERROR] Cannot connect to the database.");
    Console.ReadKey(true);
    return;
}

const int userId = 1;

// ═══════════ DAL ═══════════
IDebtRepository debtRepo = new DebtRepository(context);
IHomeRepository homeRepo = new HomeRepository(context);
IBusinessRepository busRepo = new BusinessRepository(context);
IReportRepository reportRepo = new ReportRepository(context);
ICategoryRepository catRepo = new CategoryRepository(context);

// ═══════════ BLL ═══════════
var debtService = new DebtService(debtRepo);
var homeService = new HomeService(homeRepo);
var busService = new BusinessService(busRepo);
var reportService = new ReportService(reportRepo);
var catService = new CategoryService(catRepo);

// ═══════════ PL ═══════════
var debtMenu = new DebtMenu(debtService, userId);
var homeMenu = new HomeMenu(homeService, catService, userId);
var busMenu = new BusinessMenu(busService, userId);
var reportMenu = new ReportMenu(reportService, userId);

var mainMenu = new MainMenu(debtMenu, homeMenu, busMenu, reportMenu, "Omar");

await mainMenu.ShowAsync();

Console.WriteLine("\nGoodbye.");