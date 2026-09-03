using Masareef.BusinessLayer.Services;
using Microsoft.AspNetCore.Mvc;

namespace Masareef.Api.Controllers;

[Route("api/categories")]
public class CategoriesController : ApiControllerBase
{
    private readonly CategoryService _service;
    public CategoriesController(CategoryService service) => _service = service;


    // ═══════════════════ قراءة ═══════════════════

    /// <summary>scope: 1=بيزنس 2=بيت 3=الاتنين | kind: 1=مصروف 2=دخل</summary>
    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] byte scope, [FromQuery] byte kind) =>
        FromResult(await _service.GetAsync(scope, kind, CurrentUserId));


    /// <summary>تصنيف واحد</summary>
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id) =>
        FromResult(await _service.GetByIdAsync(id, CurrentUserId));


    // ── اختصارات واضحة بدل تذكّر الأرقام ──

    /// <summary>تصنيفات مصاريف البيت</summary>
    [HttpGet("home/expenses")]
    public async Task<IActionResult> GetHomeExpenseCategories() =>
        FromResult(await _service.GetHomeExpenseCategoriesAsync(CurrentUserId));


    /// <summary>تصنيفات دخل البيت</summary>
    [HttpGet("home/incomes")]
    public async Task<IActionResult> GetHomeIncomeCategories() =>
        FromResult(await _service.GetHomeIncomeCategoriesAsync(CurrentUserId));


    /// <summary>تصنيفات مصاريف البيزنس</summary>
    [HttpGet("business/expenses")]
    public async Task<IActionResult> GetBusinessExpenseCategories() =>
        FromResult(await _service.GetBusinessExpenseCategoriesAsync(CurrentUserId));


    /// <summary>تصنيفات دخل البيزنس</summary>
    [HttpGet("business/incomes")]
    public async Task<IActionResult> GetBusinessIncomeCategories() =>
        FromResult(await _service.GetBusinessIncomeCategoriesAsync(CurrentUserId));


    // ═══════════════════ كتابة ═══════════════════

    /// <summary>إضافة تصنيف خاص بالمستخدم</summary>
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateCategoryRequest req)
    {
        var r = await _service.CreateAsync(
            CurrentUserId, req.Name, req.Icon, req.Kind, req.Scope);

        return r.Success
            ? CreatedAtAction(nameof(GetById), new { id = r.Data }, new { id = r.Data })
            : UnprocessableEntity(new { error = r.Message });
    }


    /// <summary>تعديل تصنيف — التصنيفات العامة غير قابلة للتعديل</summary>
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateCategoryRequest req) =>
        FromResult(await _service.UpdateAsync(
            id, CurrentUserId, req.Name, req.Icon, req.Kind, req.Scope));


    /// <summary>حذف تصنيف — يُرفض لو مستخدم في أي عملية</summary>
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id) =>
        FromResult(await _service.DeleteAsync(id, CurrentUserId));
}


// ═══════════════════ Request records ═══════════════════

public record CreateCategoryRequest(string Name, string? Icon, byte Kind, byte Scope);

public record UpdateCategoryRequest(string Name, string? Icon, byte Kind, byte Scope);