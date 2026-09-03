namespace Masareef.Entities.DTOs;

/// <summary>Input — إضافة منتج</summary>
public class CreateProductDto
{
    public int BusinessId { get; set; }
    public string ProductName { get; set; } = null!;
    public string? ProductCode { get; set; }
    public decimal OriginalPrice { get; set; }
    public decimal SellingPrice { get; set; }
    public int StockQuantity { get; set; }
    public string? Description { get; set; }
}

/// <summary>Input — تعديل منتج</summary>
public class UpdateProductDto
{
    public int ProductId { get; set; }
    public string ProductName { get; set; } = null!;
    public string? ProductCode { get; set; }
    public decimal OriginalPrice { get; set; }
    public decimal SellingPrice { get; set; }
    public int StockQuantity { get; set; }
    public string? Description { get; set; }
}

/// <summary>Input — تسجيل بيعة</summary>
public class CreateSaleDto
{
    public int ProductId { get; set; }
    public int QuantitySold { get; set; }
    public decimal SalePrice { get; set; }
    public DateTime SaleDate { get; set; }
    public string? CustomerName { get; set; }
    public string? Note { get; set; }
}

/// <summary>Output — صف في قائمة المنتجات مع إحصائياته</summary>
public class ProductListDto
{
    public int ProductId { get; set; }
    public int BusinessId { get; set; }
    public string ProductName { get; set; } = null!;
    public string? ProductCode { get; set; }
    public decimal OriginalPrice { get; set; }
    public decimal SellingPrice { get; set; }
    public int StockQuantity { get; set; }
    public string? Description { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }

    // ── من قاعدة البيانات ──
    public int TimesSold { get; set; }        // اتباع كام مرة
    public int TotalUnitsSold { get; set; }   // إجمالي القطع المباعة
    public decimal TotalRevenue { get; set; } // إجمالي الإيراد

    // ── محسوبة ──
    /// <summary>التكلفة الفعلية للقطع المباعة</summary>
    public decimal TotalCost => OriginalPrice * TotalUnitsSold;

    /// <summary>الربح الصافي من المنتج ده</summary>
    public decimal Profit => TotalRevenue - TotalCost;

    /// <summary>هامش الربح %</summary>
    public decimal ProfitMargin =>
        TotalRevenue == 0 ? 0 : Math.Round(Profit / TotalRevenue * 100, 1);

    /// <summary>متوسط سعر البيع الفعلي</summary>
    public decimal AverageSalePrice =>
        TotalUnitsSold == 0 ? 0 : Math.Round(TotalRevenue / TotalUnitsSold, 2);
}

/// <summary>Output — تفاصيل المنتج مع سجل مبيعاته</summary>
public class ProductDetailsDto : ProductListDto
{
    public List<SaleDto> Sales { get; set; } = new();
}

/// <summary>Output — صف بيعة واحدة</summary>
public class SaleDto
{
    public int SaleId { get; set; }
    public int QuantitySold { get; set; }
    public decimal SalePrice { get; set; }
    public DateTime SaleDate { get; set; }
    public string? CustomerName { get; set; }
    public string? Note { get; set; }

    public decimal Total => QuantitySold * SalePrice;
}


/// <summary>فلتر البحث في المنتجات</summary>
public class ProductFilterDto
{
    public string? Name { get; set; }              // بحث جزئي
    public string? ProductCode { get; set; }
    public bool? IsActive { get; set; }

    public decimal? MinPrice { get; set; }         // على SellingPrice
    public decimal? MaxPrice { get; set; }
    public decimal? MinCost { get; set; }          // على OriginalPrice
    public decimal? MaxCost { get; set; }

    public int? MinStock { get; set; }
    public int? MaxStock { get; set; }
    public bool? OutOfStock { get; set; }          // المخزون = 0
    public bool? LowStock { get; set; }            // أقل من LowStockThreshold
    public int LowStockThreshold { get; set; } = 5;

    public bool? NeverSold { get; set; }           // اللي مباعش خالص
    public string? SortBy { get; set; }            // name|profit|sold|stock|price
    public bool SortDesc { get; set; }
}

/// <summary>فلتر البحث في المبيعات</summary>
public class SaleFilterDto
{
    public int? ProductId { get; set; }
    public string? CustomerName { get; set; }

    public DateTime? DateFrom { get; set; }
    public DateTime? DateTo { get; set; }

    public decimal? MinPrice { get; set; }
    public decimal? MaxPrice { get; set; }
    public int? MinQuantity { get; set; }
    public int? MaxQuantity { get; set; }
}

/// <summary>Input — تعديل بيعة</summary>
public class UpdateSaleDto
{
    public int SaleId { get; set; }
    public int QuantitySold { get; set; }
    public decimal SalePrice { get; set; }
    public DateTime SaleDate { get; set; }
    public string? CustomerName { get; set; }
    public string? Note { get; set; }
}

/// <summary>Output — بيعة مع اسم منتجها</summary>
public class SaleListDto : SaleDto
{
    public int ProductId { get; set; }
    public string ProductName { get; set; } = null!;
    public decimal OriginalPrice { get; set; }

    /// <summary>ربح البيعة دي</summary>
    public decimal Profit => (SalePrice - OriginalPrice) * QuantitySold;
}
