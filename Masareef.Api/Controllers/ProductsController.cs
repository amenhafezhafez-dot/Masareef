using Masareef.BusinessLayer.Services;
using Masareef.Entities.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace Masareef.Api.Controllers;

[Route("api")]
public class ProductsController : ApiControllerBase
{
    private readonly ProductService _service;
    public ProductsController(ProductService service) => _service = service;


    // ═══════════════════ المنتجات — قراءة ═══════════════════

    /// <summary>كل منتجات المحل النشطة</summary>
    [HttpGet("businesses/{businessId:int}/products")]
    public async Task<IActionResult> GetByBusiness(int businessId) =>
        FromResult(await _service.GetByBusinessAsync(businessId, CurrentUserId));


    /// <summary>تفاصيل منتج مع سجل مبيعاته</summary>
    [HttpGet("products/{id:int}")]
    public async Task<IActionResult> GetById(int id) =>
        FromResult(await _service.GetDetailsAsync(id, CurrentUserId));


    /// <summary>⭐ بحث متقدّم — كل الفلاتر</summary>
    [HttpGet("businesses/{businessId:int}/products/search")]
    public async Task<IActionResult> Search(
        int businessId, [FromQuery] ProductFilterDto filter) =>
        FromResult(await _service.SearchAsync(businessId, CurrentUserId, filter));


    /// <summary>بحث بالاسم</summary>
    [HttpGet("businesses/{businessId:int}/products/search/by-name")]
    public async Task<IActionResult> SearchByName(
        int businessId, [FromQuery] string name) =>
        FromResult(await _service.SearchByNameAsync(businessId, CurrentUserId, name));


    // ── قوائم جاهزة (تنبيهات المخزون) ──

    /// <summary>المنتجات الخلصانة — مخزون صفر</summary>
    [HttpGet("businesses/{businessId:int}/products/out-of-stock")]
    public async Task<IActionResult> GetOutOfStock(int businessId) =>
        FromResult(await _service.GetOutOfStockAsync(businessId, CurrentUserId));


    /// <summary>المنتجات اللي قربت تخلص</summary>
    [HttpGet("businesses/{businessId:int}/products/low-stock")]
    public async Task<IActionResult> GetLowStock(
        int businessId, [FromQuery] int threshold = 5) =>
        FromResult(await _service.GetLowStockAsync(businessId, CurrentUserId, threshold));


    /// <summary>البضاعة الراكدة — مباعتش خالص</summary>
    [HttpGet("businesses/{businessId:int}/products/never-sold")]
    public async Task<IActionResult> GetNeverSold(int businessId) =>
        FromResult(await _service.GetNeverSoldAsync(businessId, CurrentUserId));


    /// <summary>الأكثر مبيعاً</summary>
    [HttpGet("businesses/{businessId:int}/products/best-sellers")]
    public async Task<IActionResult> GetBestSellers(int businessId) =>
        FromResult(await _service.GetBestSellersAsync(businessId, CurrentUserId));


    /// <summary>المنتجات المعطّلة</summary>
    [HttpGet("businesses/{businessId:int}/products/inactive")]
    public async Task<IActionResult> GetInactive(int businessId) =>
        FromResult(await _service.GetInactiveAsync(businessId, CurrentUserId));


    // ═══════════════════ المنتجات — كتابة ═══════════════════

    [HttpPost("businesses/{businessId:int}/products")]
    public async Task<IActionResult> Create(int businessId, [FromBody] CreateProductDto dto)
    {
        dto.BusinessId = businessId;              // 🔒 الـ URL هو مصدر الحقيقة

        var r = await _service.CreateAsync(dto, CurrentUserId);

        return r.Success
            ? CreatedAtAction(nameof(GetById), new { id = r.Data }, new { id = r.Data })
            : UnprocessableEntity(new { error = r.Message });
    }


    [HttpPut("products/{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateProductDto dto)
    {
        dto.ProductId = id;
        return FromResult(await _service.UpdateAsync(dto, CurrentUserId));
    }


    /// <summary>تعطيل — سجل المبيعات يفضل محفوظ</summary>
    [HttpDelete("products/{id:int}")]
    public async Task<IActionResult> Deactivate(int id) =>
        FromResult(await _service.DeactivateAsync(id, CurrentUserId));


    [HttpPost("products/{id:int}/reactivate")]
    public async Task<IActionResult> Reactivate(int id) =>
        FromResult(await _service.ReactivateAsync(id, CurrentUserId));


    // ═══════════════════ المبيعات — قراءة ═══════════════════

    /// <summary>⭐ كل مبيعات المحل مع بحث</summary>
    [HttpGet("businesses/{businessId:int}/sales")]
    public async Task<IActionResult> GetSales(
        int businessId, [FromQuery] SaleFilterDto filter) =>
        FromResult(await _service.SearchSalesAsync(businessId, CurrentUserId, filter));


    /// <summary>مبيعات منتج واحد</summary>
    [HttpGet("businesses/{businessId:int}/products/{productId:int}/sales")]
    public async Task<IActionResult> GetProductSales(int businessId, int productId) =>
        FromResult(await _service.GetProductSalesAsync(businessId, CurrentUserId, productId));


    /// <summary>مبيعات في فترة</summary>
    [HttpGet("businesses/{businessId:int}/sales/by-date")]
    public async Task<IActionResult> GetSalesByDate(
        int businessId, [FromQuery] DateTime? from, [FromQuery] DateTime? to) =>
        FromResult(await _service.GetSalesByDateAsync(businessId, CurrentUserId, from, to));


    // ═══════════════════ المبيعات — كتابة ═══════════════════

    /// <summary>⭐ تسجيل بيعة — بيخصم من المخزون تلقائياً</summary>
    [HttpPost("products/{id:int}/sales")]
    public async Task<IActionResult> AddSale(int id, [FromBody] CreateSaleDto dto)
    {
        dto.ProductId = id;                       // 🔒 الـ URL هو مصدر الحقيقة

        var r = await _service.AddSaleAsync(dto, CurrentUserId);

        return r.Success
            ? Created($"/api/sales/{r.Data}", new { id = r.Data })
            : UnprocessableEntity(new { error = r.Message });
    }


    /// <summary>⭐ تعديل بيعة — بيصحّح المخزون حسب فرق الكمية</summary>
    [HttpPut("sales/{saleId:int}")]
    public async Task<IActionResult> UpdateSale(int saleId, [FromBody] UpdateSaleDto dto)
    {
        dto.SaleId = saleId;
        return FromResult(await _service.UpdateSaleAsync(dto, CurrentUserId));
    }


    /// <summary>⭐ حذف بيعة — بيرجّع الكمية للمخزون</summary>
    [HttpDelete("sales/{saleId:int}")]
    public async Task<IActionResult> DeleteSale(int saleId) =>
        FromResult(await _service.DeleteSaleAsync(saleId, CurrentUserId));
}