using Masareef.BusinessLayer.Services;
using Masareef.Entities.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace Masareef.Api.Controllers;

[Route("api/home")]
public class HomeController : ApiControllerBase
{
    private readonly HomeService _service;
    public HomeController(HomeService service) => _service = service;


    // ═══════════════════ المصاريف — قراءة ═══════════════════

    /// <summary>المصاريف مع فلاتر أساسية اختيارية</summary>
    [HttpGet("expenses")]
    public async Task<IActionResult> GetExpenses(
        [FromQuery] int? year, [FromQuery] int? month, [FromQuery] int? categoryId) =>
        FromResult(await _service.GetExpensesAsync(CurrentUserId, year, month, categoryId));


    /// <summary>مصاريف الشهر الحالي — اختصار للشاشة الرئيسية</summary>
    [HttpGet("expenses/current-month")]
    public async Task<IActionResult> GetCurrentMonthExpenses() =>
        FromResult(await _service.GetCurrentMonthExpensesAsync(CurrentUserId));


    /// <summary>مصاريف في نطاق تواريخ</summary>
    [HttpGet("expenses/search/by-date-range")]
    public async Task<IActionResult> SearchExpensesByDateRange(
        [FromQuery] DateTime? from, [FromQuery] DateTime? to) =>
        FromResult(await _service.GetExpensesByDateRangeAsync(CurrentUserId, from, to));


    /// <summary>مصاريف في نطاق مبالغ</summary>
    [HttpGet("expenses/search/by-amount")]
    public async Task<IActionResult> SearchExpensesByAmount(
        [FromQuery] decimal? min, [FromQuery] decimal? max) =>
        FromResult(await _service.GetExpensesByAmountRangeAsync(CurrentUserId, min, max));


    /// <summary>بحث نصّي في وصف المصاريف</summary>
    [HttpGet("expenses/search/by-text")]
    public async Task<IActionResult> SearchExpensesByText([FromQuery] string text) =>
        FromResult(await _service.SearchExpensesByTextAsync(CurrentUserId, text));


    /// <summary>⭐ بحث متقدّم — كل الفلاتر مع بعض</summary>
    [HttpGet("expenses/search/advanced")]
    public async Task<IActionResult> AdvancedSearchExpenses([FromQuery] HomeFilterDto filter) =>
        FromResult(await _service.SearchExpensesAsync(CurrentUserId, filter));


    // ═══════════════════ المصاريف — كتابة ═══════════════════

    [HttpPost("expenses")]
    public async Task<IActionResult> AddExpense([FromBody] CreateHomeExpenseDto dto)
    {
        dto.UserId = CurrentUserId;              // 🔒 نتجاهل اللي العميل بعته

        var r = await _service.AddExpenseAsync(dto);

        return r.Success
            ? Created($"/api/home/expenses/{r.Data}", new { id = r.Data })
            : UnprocessableEntity(new { error = r.Message });
    }


    [HttpPut("expenses/{id:int}")]
    public async Task<IActionResult> UpdateExpense(
        int id, [FromBody] UpdateHomeExpenseRequest req) =>
        FromResult(await _service.UpdateExpenseAsync(
            id, CurrentUserId, req.Amount, req.ExpenseDate,
            req.CategoryId, req.PaymentMethod, req.Description));


    [HttpDelete("expenses/{id:int}")]
    public async Task<IActionResult> DeleteExpense(int id) =>
        FromResult(await _service.DeleteExpenseAsync(id, CurrentUserId));


    // ═══════════════════ الدخل — قراءة ═══════════════════

    [HttpGet("incomes")]
    public async Task<IActionResult> GetIncomes(
        [FromQuery] int? year, [FromQuery] int? month) =>
        FromResult(await _service.GetIncomesAsync(CurrentUserId, year, month));


    [HttpGet("incomes/current-month")]
    public async Task<IActionResult> GetCurrentMonthIncomes() =>
        FromResult(await _service.GetCurrentMonthIncomesAsync(CurrentUserId));


    [HttpGet("incomes/search/by-date-range")]
    public async Task<IActionResult> SearchIncomesByDateRange(
        [FromQuery] DateTime? from, [FromQuery] DateTime? to) =>
        FromResult(await _service.GetIncomesByDateRangeAsync(CurrentUserId, from, to));


    [HttpGet("incomes/search/by-amount")]
    public async Task<IActionResult> SearchIncomesByAmount(
        [FromQuery] decimal? min, [FromQuery] decimal? max) =>
        FromResult(await _service.GetIncomesByAmountRangeAsync(CurrentUserId, min, max));


    /// <summary>بحث في المصدر أو الوصف</summary>
    [HttpGet("incomes/search/by-text")]
    public async Task<IActionResult> SearchIncomesByText([FromQuery] string text) =>
        FromResult(await _service.SearchIncomesByTextAsync(CurrentUserId, text));


    [HttpGet("incomes/search/advanced")]
    public async Task<IActionResult> AdvancedSearchIncomes([FromQuery] HomeFilterDto filter) =>
        FromResult(await _service.SearchIncomesAsync(CurrentUserId, filter));


    // ═══════════════════ الدخل — كتابة ═══════════════════

    [HttpPost("incomes")]
    public async Task<IActionResult> AddIncome([FromBody] CreateHomeIncomeDto dto)
    {
        dto.UserId = CurrentUserId;

        var r = await _service.AddIncomeAsync(dto);

        return r.Success
            ? Created($"/api/home/incomes/{r.Data}", new { id = r.Data })
            : UnprocessableEntity(new { error = r.Message });
    }


    /// <summary>⭐ تعديل الدخل — كان ناقص قبل كده</summary>
    [HttpPut("incomes/{id:int}")]
    public async Task<IActionResult> UpdateIncome(
        int id, [FromBody] UpdateHomeIncomeRequest req) =>
        FromResult(await _service.UpdateIncomeAsync(
            id, CurrentUserId, req.Amount, req.IncomeDate,
            req.CategoryId, req.Source, req.Description));


    [HttpDelete("incomes/{id:int}")]
    public async Task<IActionResult> DeleteIncome(int id) =>
        FromResult(await _service.DeleteIncomeAsync(id, CurrentUserId));
}


// ═══════════════════ Request records ═══════════════════

public record UpdateHomeExpenseRequest(
    decimal Amount, DateTime ExpenseDate, int CategoryId,
    byte PaymentMethod, string? Description);

public record UpdateHomeIncomeRequest(
    decimal Amount, DateTime IncomeDate, int CategoryId,
    string? Source, string? Description);