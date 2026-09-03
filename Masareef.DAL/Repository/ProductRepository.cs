using Masareef.DAL.Context;
using Masareef.Entities.DTOs;
using Masareef.Entities.Models;
using Microsoft.EntityFrameworkCore;

namespace Masareef.DAL.Repository;

public class ProductRepository : IProductRepository
{
    private readonly AppDbContext _context;

    public ProductRepository(AppDbContext context) => _context = context;


    // ═══════════════════ المنتجات — قراءة ═══════════════════

    /// <summary>⭐ البحث المرن — كل الفلاتر بتتنفّذ في SQL</summary>
    public async Task<List<ProductListDto>> SearchAsync(int businessId, ProductFilterDto f)
    {
        var query = _context.Products
            .AsNoTracking()
            .Where(p => p.BusinessId == businessId);

        query = query.Where(p => p.IsActive == (f.IsActive ?? true));

        if (!string.IsNullOrWhiteSpace(f.Name))
            query = query.Where(p => p.ProductName.Contains(f.Name));

        if (!string.IsNullOrWhiteSpace(f.ProductCode))
            query = query.Where(p => p.ProductCode != null && p.ProductCode.Contains(f.ProductCode));

        if (f.MinPrice.HasValue) query = query.Where(p => p.SellingPrice >= f.MinPrice.Value);
        if (f.MaxPrice.HasValue) query = query.Where(p => p.SellingPrice <= f.MaxPrice.Value);
        if (f.MinCost.HasValue) query = query.Where(p => p.OriginalPrice >= f.MinCost.Value);
        if (f.MaxCost.HasValue) query = query.Where(p => p.OriginalPrice <= f.MaxCost.Value);
        if (f.MinStock.HasValue) query = query.Where(p => p.StockQuantity >= f.MinStock.Value);
        if (f.MaxStock.HasValue) query = query.Where(p => p.StockQuantity <= f.MaxStock.Value);

        if (f.OutOfStock == true)
            query = query.Where(p => p.StockQuantity == 0);

        if (f.LowStock == true)
            query = query.Where(p => p.StockQuantity > 0
                                  && p.StockQuantity <= f.LowStockThreshold);

        if (f.NeverSold == true)
            query = query.Where(p => !p.ProductSales.Any());

        var projected = query.Select(p => new ProductListDto
        {
            ProductId = p.ProductId,
            BusinessId = p.BusinessId,
            ProductName = p.ProductName,
            ProductCode = p.ProductCode,
            OriginalPrice = p.OriginalPrice,
            SellingPrice = p.SellingPrice,
            StockQuantity = p.StockQuantity,
            Description = p.Description,
            IsActive = p.IsActive,
            CreatedAt = p.CreatedAt,

            TimesSold = p.ProductSales.Count(),
            TotalUnitsSold = p.ProductSales.Sum(s => (int?)s.QuantitySold) ?? 0,
            TotalRevenue = p.ProductSales.Sum(s => (decimal?)(s.QuantitySold * s.SalePrice)) ?? 0
        });

        // ⭐ الترتيب — لاحظ إننا بنرتّب على أعمدة SQL مش properties محسوبة
        projected = f.SortBy switch
        {
            "sold" => f.SortDesc ? projected.OrderByDescending(x => x.TotalUnitsSold)
                                   : projected.OrderBy(x => x.TotalUnitsSold),
            "stock" => f.SortDesc ? projected.OrderByDescending(x => x.StockQuantity)
                                   : projected.OrderBy(x => x.StockQuantity),
            "price" => f.SortDesc ? projected.OrderByDescending(x => x.SellingPrice)
                                   : projected.OrderBy(x => x.SellingPrice),
            "profit" => f.SortDesc ? projected.OrderByDescending(x => x.TotalRevenue)
                                   : projected.OrderBy(x => x.TotalRevenue),
            _ => f.SortDesc ? projected.OrderByDescending(x => x.ProductName)
                                   : projected.OrderBy(x => x.ProductName),
        };

        return await projected.ToListAsync();
    }


    public async Task<ProductDetailsDto?> GetDetailsAsync(int productId) =>
        await _context.Products
            .AsNoTracking()
            .Where(p => p.ProductId == productId)
            .Select(p => new ProductDetailsDto
            {
                ProductId = p.ProductId,
                BusinessId = p.BusinessId,
                ProductName = p.ProductName,
                ProductCode = p.ProductCode,
                OriginalPrice = p.OriginalPrice,
                SellingPrice = p.SellingPrice,
                StockQuantity = p.StockQuantity,
                Description = p.Description,
                IsActive = p.IsActive,
                CreatedAt = p.CreatedAt,

                TimesSold = p.ProductSales.Count(),
                TotalUnitsSold = p.ProductSales.Sum(s => (int?)s.QuantitySold) ?? 0,
                TotalRevenue = p.ProductSales.Sum(s => (decimal?)(s.QuantitySold * s.SalePrice)) ?? 0,

                Sales = p.ProductSales
                    .OrderByDescending(s => s.SaleDate)
                    .Select(s => new SaleDto
                    {
                        SaleId = s.SaleId,
                        QuantitySold = s.QuantitySold,
                        SalePrice = s.SalePrice,
                        SaleDate = s.SaleDate,
                        CustomerName = s.CustomerName,
                        Note = s.Note
                    }).ToList()
            })
            .FirstOrDefaultAsync();


    public async Task<Product?> GetByIdAsync(int productId) =>
        await _context.Products.FirstOrDefaultAsync(p => p.ProductId == productId);


    /// <summary>⭐ رقمين بس — من غير تحميل الكيان (زي GetStatusAndPaidAsync في الديون)</summary>
    public async Task<(int Stock, decimal OriginalPrice)?> GetStockAndCostAsync(int productId)
    {
        var row = await _context.Products
            .AsNoTracking()
            .Where(p => p.ProductId == productId)
            .Select(p => new { p.StockQuantity, p.OriginalPrice })
            .FirstOrDefaultAsync();

        return row is null ? null : (row.StockQuantity, row.OriginalPrice);
    }


    // ═══════════════════ المنتجات — كتابة ═══════════════════

    public async Task<int> AddAsync(Product product)
    {
        _context.Products.Add(product);
        await _context.SaveChangesAsync();
        return product.ProductId;
    }


    /// <summary>⭐ بتاخد بارامترات مش DTO — الـ DAL مايعرفش عن DTOs الإدخال</summary>
    public async Task<bool> UpdateAsync(int productId, string name, string? code,
        decimal originalPrice, decimal sellingPrice, int stock, string? description)
    {
        var p = await _context.Products.FirstOrDefaultAsync(x => x.ProductId == productId);
        if (p is null) return false;

        p.ProductName = name;
        p.ProductCode = code;
        p.OriginalPrice = originalPrice;
        p.SellingPrice = sellingPrice;
        p.StockQuantity = stock;
        p.Description = description;

        return await _context.SaveChangesAsync() > 0;
    }


    public async Task<bool> DeactivateAsync(int productId)
    {
        var p = await _context.Products.FirstOrDefaultAsync(x => x.ProductId == productId);
        if (p is null) return false;

        p.IsActive = false;   // soft delete — تاريخ المبيعات يفضل محفوظ
        return await _context.SaveChangesAsync() > 0;
    }


    public async Task<bool> ReactivateAsync(int productId)
    {
        var p = await _context.Products.FirstOrDefaultAsync(x => x.ProductId == productId);
        if (p is null) return false;

        p.IsActive = true;
        return await _context.SaveChangesAsync() > 0;
    }


    // ═══════════════════ المبيعات ═══════════════════

    public async Task<List<SaleListDto>> SearchSalesAsync(int businessId, SaleFilterDto f)
    {
        var query = _context.ProductSales
            .AsNoTracking()
            .Where(s => s.Product.BusinessId == businessId);

        if (f.ProductId.HasValue)
            query = query.Where(s => s.ProductId == f.ProductId.Value);

        if (!string.IsNullOrWhiteSpace(f.CustomerName))
            query = query.Where(s => s.CustomerName != null
                                  && s.CustomerName.Contains(f.CustomerName));

        if (f.DateFrom.HasValue) query = query.Where(s => s.SaleDate >= f.DateFrom.Value);
        if (f.DateTo.HasValue) query = query.Where(s => s.SaleDate <= f.DateTo.Value);
        if (f.MinPrice.HasValue) query = query.Where(s => s.SalePrice >= f.MinPrice.Value);
        if (f.MaxPrice.HasValue) query = query.Where(s => s.SalePrice <= f.MaxPrice.Value);
        if (f.MinQuantity.HasValue) query = query.Where(s => s.QuantitySold >= f.MinQuantity.Value);
        if (f.MaxQuantity.HasValue) query = query.Where(s => s.QuantitySold <= f.MaxQuantity.Value);

        return await query
            .Select(s => new SaleListDto
            {
                SaleId = s.SaleId,
                ProductId = s.ProductId,
                ProductName = s.Product.ProductName,
                OriginalPrice = s.Product.OriginalPrice,
                QuantitySold = s.QuantitySold,
                SalePrice = s.SalePrice,
                SaleDate = s.SaleDate,
                CustomerName = s.CustomerName,
                Note = s.Note
            })
            .OrderByDescending(s => s.SaleDate)
            .ThenByDescending(s => s.SaleId)
            .ToListAsync();
    }


    public async Task<ProductSale?> GetSaleByIdAsync(int saleId) =>
        await _context.ProductSales.FirstOrDefaultAsync(s => s.SaleId == saleId);


    /// <summary>⭐ البيعة وتحديث المخزون في حفظة واحدة</summary>
    public async Task<int> AddSaleAsync(ProductSale sale, int newStock)
    {
        _context.ProductSales.Add(sale);

        var product = await _context.Products.FindAsync(sale.ProductId);
        if (product is not null)
            product.StockQuantity = newStock;

        await _context.SaveChangesAsync();   // الاتنين يا يتحفظوا يا لأ
        return sale.SaleId;
    }


    /// <summary>⭐ تعديل البيعة + تصحيح المخزون — حفظة واحدة</summary>
    public async Task<bool> UpdateSaleAsync(UpdateSaleDto dto, int newStock)
    {
        var sale = await _context.ProductSales.FirstOrDefaultAsync(s => s.SaleId == dto.SaleId);
        if (sale is null) return false;

        sale.QuantitySold = dto.QuantitySold;
        sale.SalePrice = dto.SalePrice;
        sale.SaleDate = dto.SaleDate.Date;
        sale.CustomerName = dto.CustomerName;
        sale.Note = dto.Note;

        var product = await _context.Products.FindAsync(sale.ProductId);
        if (product is not null)
            product.StockQuantity = newStock;

        return await _context.SaveChangesAsync() > 0;
    }


    /// <summary>⭐ حذف البيعة + إرجاع الكمية للمخزون — حفظة واحدة</summary>
    public async Task<bool> DeleteSaleAsync(int saleId, int newStock)
    {
        var sale = await _context.ProductSales.FirstOrDefaultAsync(s => s.SaleId == saleId);
        if (sale is null) return false;

        var product = await _context.Products.FindAsync(sale.ProductId);
        if (product is not null)
            product.StockQuantity = newStock;

        _context.ProductSales.Remove(sale);

        return await _context.SaveChangesAsync() > 0;
    }


    // ═══════════════════ فحوصات ═══════════════════

    public async Task<bool> ProductBelongsToUserAsync(int productId, int userId) =>
        await _context.Products
            .AsNoTracking()
            .AnyAsync(p => p.ProductId == productId && p.Business.UserId == userId);


    public async Task<bool> SaleBelongsToUserAsync(int saleId, int userId) =>
        await _context.ProductSales
            .AsNoTracking()
            .AnyAsync(s => s.SaleId == saleId && s.Product.Business.UserId == userId);


    public async Task<bool> NameExistsAsync(string name, int businessId, int? excludeId = null)
    {
        var query = _context.Products
            .AsNoTracking()
            .Where(p => p.ProductName == name && p.BusinessId == businessId);

        if (excludeId.HasValue)
            query = query.Where(p => p.ProductId != excludeId.Value);

        return await query.AnyAsync();
    }


    public async Task<bool> HasSalesAsync(int productId) =>
        await _context.ProductSales
            .AsNoTracking()
            .AnyAsync(s => s.ProductId == productId);
}