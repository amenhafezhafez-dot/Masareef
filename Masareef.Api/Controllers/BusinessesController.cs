using Masareef.BusinessLayer.Services;
using Masareef.Entities.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace Masareef.Api.Controllers;

[Route("api/businesses")]
public class BusinessesController : ApiControllerBase
{
    private readonly BusinessService _service;
    public BusinessesController(BusinessService service) => _service = service;


    // ═══════════════════ المحلات — قراءة ═══════════════════

    /// <summary>المحلات النشطة</summary>
    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken ct) =>
        FromResult(await _service.GetAllAsync(CurrentUserId, ct));


    /// <summary>محل واحد بالتفصيل</summary>
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id, CancellationToken ct) =>
        FromResult(await _service.GetByIdAsync(id, CurrentUserId, ct));


    /// <summary>المحلات المعطّلة</summary>
    [HttpGet("inactive")]
    public async Task<IActionResult> GetInactive(CancellationToken ct) =>
        FromResult(await _service.GetInactiveAsync(CurrentUserId, ct));


    /// <summary>بحث بالاسم</summary>
    [HttpGet("search/by-name")]
    public async Task<IActionResult> SearchByName(
        [FromQuery] string name, CancellationToken ct) =>
        FromResult(await _service.SearchByNameAsync(CurrentUserId, name, ct));


    /// <summary>⭐ بحث متقدّم — كل الفلاتر</summary>
    [HttpGet("search/advanced")]
    public async Task<IActionResult> AdvancedSearch(
        [FromQuery] BusinessFilterDto filter, CancellationToken ct) =>
        FromResult(await _service.SearchAsync(CurrentUserId, filter, ct));


    // ═══════════════════ الأرباح ═══════════════════

    /// <summary>أرباح محل في شهر</summary>
    [HttpGet("{id:int}/profit")]
    public async Task<IActionResult> GetProfit(
        int id, [FromQuery] int? year, [FromQuery] int? month, CancellationToken ct) =>
        FromResult(await _service.GetProfitAsync(
            id, CurrentUserId,
            year ?? DateTime.Today.Year,
            month ?? DateTime.Today.Month, ct));


    /// <summary>أرباح كل المحلات — للمقارنة</summary>
    [HttpGet("profits")]
    public async Task<IActionResult> GetAllProfits(
        [FromQuery] int? year, [FromQuery] int? month, CancellationToken ct) =>
        FromResult(await _service.GetAllProfitsAsync(
            CurrentUserId,
            year ?? DateTime.Today.Year,
            month ?? DateTime.Today.Month, ct));


    /// <summary>أرباح المحل منذ إنشائه</summary>
    [HttpGet("{id:int}/profit/all-time")]
    public async Task<IActionResult> GetLifetimeProfit(int id) =>
        FromResult(await _service.GetLifetimeProfitAsync(id, CurrentUserId));


    // ═══════════════════ المحلات — كتابة ═══════════════════

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] BusinessRequest req)
    {
        var r = await _service.CreateAsync(
            CurrentUserId, req.Name, req.Description, req.Currency);

        return r.Success
            ? CreatedAtAction(nameof(GetById), new { id = r.Data }, new { id = r.Data })
            : UnprocessableEntity(new { error = r.Message });
    }


    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] BusinessRequest req) =>
        FromResult(await _service.UpdateAsync(
            id, CurrentUserId, req.Name, req.Description, req.Currency));


    /// <summary>تعطيل — التاريخ المالي يفضل محفوظ</summary>
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Deactivate(int id) =>
        FromResult(await _service.DeactivateAsync(id, CurrentUserId));


    /// <summary>إعادة تفعيل محل معطّل</summary>
    [HttpPost("{id:int}/reactivate")]
    public async Task<IActionResult> Reactivate(int id) =>
        FromResult(await _service.ReactivateAsync(id, CurrentUserId));


    // ═══════════════════ المصاريف ═══════════════════

    [HttpGet("{id:int}/expenses")]
    public async Task<IActionResult> GetExpenses(
        int id, [FromQuery] int? year, [FromQuery] int? month, CancellationToken ct) =>
        FromResult(await _service.GetExpensesAsync(
            id, CurrentUserId,
            new BusinessTransactionFilterDto { Year = year, Month = month }, ct));


    /// <summary>⭐ بحث متقدّم في المصاريف</summary>
    [HttpGet("{id:int}/expenses/search")]
    public async Task<IActionResult> SearchExpenses(
        int id, [FromQuery] BusinessTransactionFilterDto filter, CancellationToken ct) =>
        FromResult(await _service.GetExpensesAsync(id, CurrentUserId, filter, ct));


    [HttpPost("{id:int}/expenses")]
    public async Task<IActionResult> AddExpense(
        int id, [FromBody] CreateBusinessExpenseDto dto)
    {
        dto.BusinessId = id;                      // 🔒 الـ URL هو مصدر الحقيقة

        var r = await _service.AddExpenseAsync(dto, CurrentUserId);

        return r.Success
            ? Created($"/api/businesses/{id}/expenses/{r.Data}", new { id = r.Data })
            : UnprocessableEntity(new { error = r.Message });
    }


    [HttpPut("expenses/{expenseId:int}")]
    public async Task<IActionResult> UpdateExpense(
        int expenseId, [FromBody] UpdateBusinessExpenseRequest req) =>
        FromResult(await _service.UpdateExpenseAsync(
            expenseId, CurrentUserId, req.Amount, req.ExpenseDate,
            req.CategoryId, req.PaymentMethod, req.Description));


    [HttpDelete("expenses/{expenseId:int}")]
    public async Task<IActionResult> DeleteExpense(int expenseId) =>
        FromResult(await _service.DeleteExpenseAsync(expenseId, CurrentUserId));


    // ═══════════════════ الدخل ═══════════════════

    [HttpGet("{id:int}/incomes")]
    public async Task<IActionResult> GetIncomes(
        int id, [FromQuery] int? year, [FromQuery] int? month, CancellationToken ct) =>
        FromResult(await _service.GetIncomesAsync(
            id, CurrentUserId,
            new BusinessTransactionFilterDto { Year = year, Month = month }, ct));


    [HttpGet("{id:int}/incomes/search")]
    public async Task<IActionResult> SearchIncomes(
        int id, [FromQuery] BusinessTransactionFilterDto filter, CancellationToken ct) =>
        FromResult(await _service.GetIncomesAsync(id, CurrentUserId, filter, ct));


    [HttpPost("{id:int}/incomes")]
    public async Task<IActionResult> AddIncome(
        int id, [FromBody] CreateBusinessIncomeDto dto)
    {
        dto.BusinessId = id;

        var r = await _service.AddIncomeAsync(dto, CurrentUserId);

        return r.Success
            ? Created($"/api/businesses/{id}/incomes/{r.Data}", new { id = r.Data })
            : UnprocessableEntity(new { error = r.Message });
    }


    [HttpPut("incomes/{incomeId:int}")]
    public async Task<IActionResult> UpdateIncome(
        int incomeId, [FromBody] UpdateBusinessIncomeRequest req) =>
        FromResult(await _service.UpdateIncomeAsync(
            incomeId, CurrentUserId, req.Amount, req.IncomeDate,
            req.CategoryId, req.Source, req.Description));


    [HttpDelete("incomes/{incomeId:int}")]
    public async Task<IActionResult> DeleteIncome(int incomeId) =>
        FromResult(await _service.DeleteIncomeAsync(incomeId, CurrentUserId));
}


// ═══════════════════ Request records ═══════════════════

public record BusinessRequest(string Name, string? Description, string Currency);

public record UpdateBusinessExpenseRequest(
    decimal Amount, DateTime ExpenseDate, int CategoryId,
    byte PaymentMethod, string? Description);

public record UpdateBusinessIncomeRequest(
    decimal Amount, DateTime IncomeDate, int CategoryId,
    string? Source, string? Description);