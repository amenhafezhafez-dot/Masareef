using Masareef.Entities.DTOs;
using Masareef.Entities.Models;

namespace Masareef.DAL.Repository;

public interface IProductRepository
{
    // ═══════════ المنتجات — قراءة ═══════════
    /// <summary>⭐ البحث المرن — كل الحالات</summary>
    Task<List<ProductListDto>> SearchAsync(int businessId, ProductFilterDto filter);

    Task<ProductDetailsDto?> GetDetailsAsync(int productId);
    Task<Product?> GetByIdAsync(int productId);

    /// <summary>السعر والمخزون بس — للفحوصات، من غير تحميل الكيان</summary>
    Task<(int Stock, decimal OriginalPrice)?> GetStockAndCostAsync(int productId);

    // ═══════════ المنتجات — كتابة ═══════════
    Task<int> AddAsync(Product product);

    Task<bool> UpdateAsync(int productId, string name, string? code,
        decimal originalPrice, decimal sellingPrice, int stock, string? description);

    Task<bool> DeactivateAsync(int productId);
    Task<bool> ReactivateAsync(int productId);

    // ═══════════ المبيعات ═══════════
    Task<List<SaleListDto>> SearchSalesAsync(int businessId, SaleFilterDto filter);
    Task<ProductSale?> GetSaleByIdAsync(int saleId);

    /// <summary>البيعة + تحديث المخزون في حفظة واحدة</summary>
    Task<int> AddSaleAsync(ProductSale sale, int newStock);

    /// <summary>تعديل البيعة + تصحيح المخزون في حفظة واحدة</summary>
    Task<bool> UpdateSaleAsync(UpdateSaleDto dto, int newStock);

    /// <summary>حذف البيعة + إرجاع الكمية للمخزون</summary>
    Task<bool> DeleteSaleAsync(int saleId, int newStock);

    // ═══════════ فحوصات ═══════════
    Task<bool> ProductBelongsToUserAsync(int productId, int userId);
    Task<bool> SaleBelongsToUserAsync(int saleId, int userId);
    Task<bool> NameExistsAsync(string name, int businessId, int? excludeId = null);
    Task<bool> HasSalesAsync(int productId);
}