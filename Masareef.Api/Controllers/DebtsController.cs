using Masareef.BusinessLayer.Services;
using Masareef.Entities.DTOs;
using Masareef.MasareefEntities.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace Masareef.Api.Controllers;

[Route("api/debts")]
public class DebtsController : ApiControllerBase
{
    private readonly DebtService _service;
    public DebtsController(DebtService service) => _service = service;



    /// <summary>كل الديون — مع فلتر اختياري بالحالة</summary>
    [HttpGet]
    
    public async Task<IActionResult> GetAll([FromQuery] byte? status) =>
        FromResult(await _service.GetAllAsync(CurrentUserId, status));


    /// <summary>تفاصيل دين واحد مع سجل دفعاته</summary>
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id) =>
        FromResult(await _service.GetDetailsAsync(id, CurrentUserId));


    /// <summary>إجماليات: عليّ / ليّ / الصافي / عدد المفتوح والمتأخر</summary>
    [HttpGet("summary/totals")]
    public async Task<IActionResult> GetTotals() =>
        FromResult(await _service.GetTotalsAsync(CurrentUserId));


    // ═══════════════════ قوائم جاهزة (اختصارات) ═══════════════════

    /// <summary>الديون المتأخرة — تجاوزت تاريخ الاستحقاق ولسه مش مسدّدة</summary>
    [HttpGet("overdue")]
    public async Task<IActionResult> GetOverdue() =>
        FromResult(await _service.GetOverdueAsync(CurrentUserId));


    /// <summary>الديون غير المنتهية — المفتوحة والمسدّدة جزئياً</summary>
    [HttpGet("pending")]
    public async Task<IActionResult> GetPending() =>
        FromResult(await _service.GetNotDoneAsync(CurrentUserId));


    /// <summary>الديون اللي عليّ (أنا المدين)</summary>
    [HttpGet("i-owe")]
    public async Task<IActionResult> GetWhatIOwe() =>
        FromResult(await _service.SearchAsync(
            CurrentUserId, new DebtFilterDto { Direction = 1 }));


    /// <summary>الديون اللي ليّ (الآخرون مدينون لي)</summary>
    [HttpGet("owed-to-me")]
    public async Task<IActionResult> GetWhatIsOwedToMe() =>
        FromResult(await _service.SearchAsync(
            CurrentUserId, new DebtFilterDto { Direction = 2 }));


    // ═══════════════════ بحث ═══════════════════

    /// <summary>بحث سريع بالاسم — للـ search box</summary>
    [HttpGet("search/by-person")]
    public async Task<IActionResult> SearchByPerson([FromQuery] string name) =>
        FromResult(await _service.SearchByNameAsync(CurrentUserId, name));


    /// <summary>بحث في نطاق مبالغ</summary>
    [HttpGet("search/by-amount")]
    public async Task<IActionResult> SearchByAmount(
        [FromQuery] decimal? min, [FromQuery] decimal? max) =>
        FromResult(await _service.SearchAsync(
            CurrentUserId, new DebtFilterDto { MinAmount = min, MaxAmount = max }));


    /// <summary>بحث في نطاق تواريخ الاستحقاق</summary>
    [HttpGet("search/by-due-date")]
    public async Task<IActionResult> SearchByDueDate(
        [FromQuery] DateTime? from, [FromQuery] DateTime? to) =>
        FromResult(await _service.SearchAsync(
            CurrentUserId, new DebtFilterDto { DueFrom = from, DueTo = to }));


    /// <summary>بحث في نطاق تواريخ إنشاء الدين</summary>
    [HttpGet("search/by-debt-date")]
    public async Task<IActionResult> SearchByDebtDate(
        [FromQuery] DateTime? from, [FromQuery] DateTime? to) =>
        FromResult(await _service.SearchAsync(
            CurrentUserId, new DebtFilterDto { DebtFrom = from, DebtTo = to }));


    /// <summary>⭐ بحث متقدّم — كل الفلاتر مع بعض</summary>
    [HttpGet("search/advanced")]
    public async Task<IActionResult> AdvancedSearch([FromQuery] DebtFilterDto filter) =>
        FromResult(await _service.SearchAsync(CurrentUserId, filter));


    // ═══════════════════ إنشاء وتعديل ═══════════════════

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateDebtDto dto)
    {
        dto.UserID = CurrentUserId;              // 🔒 نتجاهل اللي العميل بعته

        var r = await _service.CreateAsync(dto);

        return r.Success
            ? CreatedAtAction(nameof(GetById), new { id = r.Data }, new { id = r.Data })
            : UnprocessableEntity(new { error = r.Message });
    }


    /// <summary>تعديل بيانات الدين — الاسم، الهاتف، المبلغ، الاستحقاق</summary>
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateDebtDto dto) =>
        FromResult(await _service.UpdateDeptAsync(
            id, CurrentUserId, dto.PersonName, dto.PersonPhone,
            dto.Amount, dto.DueDate, dto.Description));


    // ═══════════════════ عمليات ═══════════════════

    /// <summary>⭐ تسجيل دفعة — بيحدّث حالة الدين تلقائياً</summary>
    [HttpPost("{id:int}/payments")]
    public async Task<IActionResult> AddPayment(int id, [FromBody] AddPaymentDto dto)
    {
        dto.DebtID = id;                         // 🔒 الـ URL هو مصدر الحقيقة

        var r = await _service.AddPaymentAsync(dto, CurrentUserId);

        return r.Success
            ? Created($"/api/debts/{id}/payments/{r.Data}", new { id = r.Data })
            : UnprocessableEntity(new { error = r.Message });
    }


    /// <summary>شطب الدين — بيتسجّل السبب ومينفعش يترجع</summary>
    [HttpPost("{id:int}/write-off")]
    public async Task<IActionResult> WriteOff(int id, [FromBody] WriteOffRequest req) =>
        FromResult(await _service.WriteOffAsync(id, CurrentUserId, req.Reason));
}

public record WriteOffRequest(string Reason);