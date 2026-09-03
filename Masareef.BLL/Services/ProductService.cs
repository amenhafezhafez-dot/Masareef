using Masareef.DAL.Repository;
using Masareef.Entities.Common;
using Masareef.Entities.DTOs;
using Masareef.Entities.Models;

namespace Masareef.BusinessLayer.Services;

public class ProductService
{
    private readonly IProductRepository _repo;
    private readonly IBusinessRepository _businessRepo;

    public ProductService(IProductRepository repo, IBusinessRepository businessRepo)
    {
        _repo = repo;
        _businessRepo = businessRepo;
    }


    // ═══════════════════ المنتجات — قراءة ═══════════════════

    /// <summary>⭐ البحث الأساسي</summary>
    public async Task<Result<List<ProductListDto>>> SearchAsync(
        int businessId, int userId, ProductFilterDto filter)
    {
        // 🔒 المحل تبع اليوزر ده؟
        if (!await _businessRepo.BusinessBelongsToUserAsync(businessId, userId))
            return Result<List<ProductListDto>>.Fail("Business not found.");

        var error = ValidateFilter(filter);
        if (error is not null)
            return Result<List<ProductListDto>>.Fail(error);

        return Result<List<ProductListDto>>.Ok(await _repo.SearchAsync(businessId, filter));
    }


    // ── اختصارات ──

    public Task<Result<List<ProductListDto>>> GetByBusinessAsync(int businessId, int userId) =>
        SearchAsync(businessId, userId, new ProductFilterDto());

    /// <summary>المنتجات الخلصانة</summary>
    public Task<Result<List<ProductListDto>>> GetOutOfStockAsync(int businessId, int userId) =>
        SearchAsync(businessId, userId, new ProductFilterDto { OutOfStock = true });

    /// <summary>المنتجات اللي قربت تخلص</summary>
    public Task<Result<List<ProductListDto>>> GetLowStockAsync(
        int businessId, int userId, int threshold = 5) =>
        SearchAsync(businessId, userId,
            new ProductFilterDto { LowStock = true, LowStockThreshold = threshold });

    /// <summary>البضاعة الراكدة — مباعتش خالص</summary>
    public Task<Result<List<ProductListDto>>> GetNeverSoldAsync(int businessId, int userId) =>
        SearchAsync(businessId, userId, new ProductFilterDto { NeverSold = true });

    /// <summary>الأكثر مبيعاً</summary>
    public Task<Result<List<ProductListDto>>> GetBestSellersAsync(int businessId, int userId) =>
        SearchAsync(businessId, userId,
            new ProductFilterDto { SortBy = "sold", SortDesc = true });

    /// <summary>المنتجات المعطّلة</summary>
    public Task<Result<List<ProductListDto>>> GetInactiveAsync(int businessId, int userId) =>
        SearchAsync(businessId, userId, new ProductFilterDto { IsActive = false });

    public Task<Result<List<ProductListDto>>> SearchByNameAsync(
        int businessId, int userId, string name) =>
        string.IsNullOrWhiteSpace(name) || name.Trim().Length < 2
            ? Task.FromResult(Result<List<ProductListDto>>.Fail(
                "Search text must be at least 2 characters."))
            : SearchAsync(businessId, userId, new ProductFilterDto { Name = name.Trim() });


    public async Task<Result<ProductDetailsDto>> GetDetailsAsync(int productId, int userId)
    {
        if (productId <= 0)
            return Result<ProductDetailsDto>.Fail("Invalid product ID.");

        if (!await _repo.ProductBelongsToUserAsync(productId, userId))
            return Result<ProductDetailsDto>.Fail("Product not found.");

        var dto = await _repo.GetDetailsAsync(productId);

        return dto is null
            ? Result<ProductDetailsDto>.Fail("Product not found.")
            : Result<ProductDetailsDto>.Ok(dto);
    }


    // ═══════════════════ المنتجات — كتابة ═══════════════════

    public async Task<Result<int>> CreateAsync(CreateProductDto dto, int userId)
    {
        if (!await _businessRepo.BusinessBelongsToUserAsync(dto.BusinessId, userId))
            return Result<int>.Fail("Business not found.");

        var error = ValidateProductInput(
            dto.ProductName, dto.OriginalPrice, dto.SellingPrice, dto.StockQuantity);
        if (error is not null) return Result<int>.Fail(error);

        // ⭐ اسم مكرر في نفس المحل؟
        if (await _repo.NameExistsAsync(dto.ProductName.Trim(), dto.BusinessId))
            return Result<int>.Fail("A product with this name already exists in this business.");

        var product = new Product
        {
            BusinessId = dto.BusinessId,
            ProductName = dto.ProductName.Trim(),
            ProductCode = string.IsNullOrWhiteSpace(dto.ProductCode) ? null : dto.ProductCode.Trim(),
            OriginalPrice = dto.OriginalPrice,
            SellingPrice = dto.SellingPrice,
            StockQuantity = dto.StockQuantity,
            Description = dto.Description?.Trim(),

            // من النظام
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        return Result<int>.Ok(await _repo.AddAsync(product));
    }


    public async Task<Result> UpdateAsync(UpdateProductDto dto, int userId)
    {
        if (dto.ProductId <= 0)
            return Result.Fail("Invalid product ID.");

        // 🔒 المنتج تبع محل تبع اليوزر ده؟
        if (!await _repo.ProductBelongsToUserAsync(dto.ProductId, userId))
            return Result.Fail("Product not found.");

        var error = ValidateProductInput(
            dto.ProductName, dto.OriginalPrice, dto.SellingPrice, dto.StockQuantity);
        if (error is not null) return Result.Fail(error);

        // الـ BusinessId بنجيبه من المنتج نفسه — مش من العميل
        var product = await _repo.GetByIdAsync(dto.ProductId);
        if (product is null) return Result.Fail("Product not found.");

        if (await _repo.NameExistsAsync(dto.ProductName.Trim(), product.BusinessId, dto.ProductId))
            return Result.Fail("Another product with this name already exists.");

        bool ok = await _repo.UpdateAsync(
            dto.ProductId,
            dto.ProductName.Trim(),
            string.IsNullOrWhiteSpace(dto.ProductCode) ? null : dto.ProductCode.Trim(),
            dto.OriginalPrice,
            dto.SellingPrice,
            dto.StockQuantity,
            dto.Description?.Trim());

        return ok ? Result.Ok() : Result.Fail("Update failed.");
    }


    public async Task<Result> DeactivateAsync(int productId, int userId)
    {
        if (!await _repo.ProductBelongsToUserAsync(productId, userId))
            return Result.Fail("Product not found.");

        bool ok = await _repo.DeactivateAsync(productId);
        return ok ? Result.Ok() : Result.Fail("Deactivation failed.");
    }


    public async Task<Result> ReactivateAsync(int productId, int userId)
    {
        if (!await _repo.ProductBelongsToUserAsync(productId, userId))
            return Result.Fail("Product not found.");

        bool ok = await _repo.ReactivateAsync(productId);
        return ok ? Result.Ok() : Result.Fail("Reactivation failed.");
    }


    // ═══════════════════ المبيعات — قراءة ═══════════════════

    public async Task<Result<List<SaleListDto>>> SearchSalesAsync(
        int businessId, int userId, SaleFilterDto filter)
    {
        if (!await _businessRepo.BusinessBelongsToUserAsync(businessId, userId))
            return Result<List<SaleListDto>>.Fail("Business not found.");

        if (filter.MinPrice.HasValue && filter.MaxPrice.HasValue
            && filter.MinPrice > filter.MaxPrice)
            return Result<List<SaleListDto>>.Fail("Min price cannot exceed max price.");

        if (filter.DateFrom.HasValue && filter.DateTo.HasValue
            && filter.DateFrom > filter.DateTo)
            return Result<List<SaleListDto>>.Fail("Invalid date range.");

        return Result<List<SaleListDto>>.Ok(await _repo.SearchSalesAsync(businessId, filter));
    }


    /// <summary>مبيعات منتج واحد</summary>
    public Task<Result<List<SaleListDto>>> GetProductSalesAsync(
        int businessId, int userId, int productId) =>
        SearchSalesAsync(businessId, userId, new SaleFilterDto { ProductId = productId });


    /// <summary>مبيعات في فترة</summary>
    public Task<Result<List<SaleListDto>>> GetSalesByDateAsync(
        int businessId, int userId, DateTime? from, DateTime? to) =>
        SearchSalesAsync(businessId, userId, new SaleFilterDto { DateFrom = from, DateTo = to });


    // ═══════════════════ ⭐ المبيعات — كتابة ═══════════════════

    public async Task<Result<int>> AddSaleAsync(CreateSaleDto dto, int userId)
    {
        if (!await _repo.ProductBelongsToUserAsync(dto.ProductId, userId))
            return Result<int>.Fail("Product not found.");

        // ⭐ المخزون والتكلفة — رقمين بس، من غير تحميل الكيان
        var info = await _repo.GetStockAndCostAsync(dto.ProductId);
        if (info is null)
            return Result<int>.Fail("Product not found.");

        var (stock, _) = info.Value;

        var error = ValidateSaleInput(dto.QuantitySold, dto.SalePrice, dto.SaleDate);
        if (error is not null) return Result<int>.Fail(error);

        // 🔴 القاعدة الحرجة — مينفعش تبيع أكتر من المتاح
        if (dto.QuantitySold > stock)
            return Result<int>.Fail(
                $"Quantity ({dto.QuantitySold}) exceeds available stock ({stock}).");

        var sale = new ProductSale
        {
            ProductId = dto.ProductId,
            QuantitySold = dto.QuantitySold,
            SalePrice = dto.SalePrice,
            SaleDate = dto.SaleDate.Date,
            CustomerName = dto.CustomerName?.Trim(),
            Note = dto.Note?.Trim(),
            CreatedAt = DateTime.UtcNow
        };

        int newStock = stock - dto.QuantitySold;

        return Result<int>.Ok(await _repo.AddSaleAsync(sale, newStock));
    }


    /// <summary>⭐ تعديل بيعة — بيصحّح المخزون حسب الفرق</summary>
    public async Task<Result> UpdateSaleAsync(UpdateSaleDto dto, int userId)
    {
        if (dto.SaleId <= 0)
            return Result.Fail("Invalid sale ID.");

        if (!await _repo.SaleBelongsToUserAsync(dto.SaleId, userId))
            return Result.Fail("Sale not found.");

        var error = ValidateSaleInput(dto.QuantitySold, dto.SalePrice, dto.SaleDate);
        if (error is not null) return Result.Fail(error);

        var sale = await _repo.GetSaleByIdAsync(dto.SaleId);
        if (sale is null) return Result.Fail("Sale not found.");

        var info = await _repo.GetStockAndCostAsync(sale.ProductId);
        if (info is null) return Result.Fail("Product not found.");

        var (currentStock, _) = info.Value;

        // ⭐ الحساب المهم:
        // المخزون الحالي متحسب بعد خصم الكمية القديمة.
        // فعشان نطبّق الكمية الجديدة: نرجّع القديمة، ونخصم الجديدة.
        int available = currentStock + sale.QuantitySold;   // المخزون لو البيعة دي متحصلتش
        int newStock = available - dto.QuantitySold;

        if (newStock < 0)
            return Result.Fail(
                $"New quantity ({dto.QuantitySold}) exceeds available stock ({available}).");

        bool ok = await _repo.UpdateSaleAsync(dto, newStock);

        return ok ? Result.Ok() : Result.Fail("Update failed.");
    }


    /// <summary>⭐ حذف بيعة — بيرجّع الكمية للمخزون</summary>
    public async Task<Result> DeleteSaleAsync(int saleId, int userId)
    {
        if (saleId <= 0)
            return Result.Fail("Invalid sale ID.");

        if (!await _repo.SaleBelongsToUserAsync(saleId, userId))
            return Result.Fail("Sale not found.");

        var sale = await _repo.GetSaleByIdAsync(saleId);
        if (sale is null) return Result.Fail("Sale not found.");

        var info = await _repo.GetStockAndCostAsync(sale.ProductId);
        if (info is null) return Result.Fail("Product not found.");

        var (currentStock, _) = info.Value;

        // الكمية بترجع للمخزون
        int newStock = currentStock + sale.QuantitySold;

        bool ok = await _repo.DeleteSaleAsync(saleId, newStock);

        return ok ? Result.Ok() : Result.Fail("Delete failed.");
    }


    // ═══════════════════ فحوصات مشتركة ═══════════════════

    private static string? ValidateProductInput(
        string name, decimal originalPrice, decimal sellingPrice, int stock)
    {
        if (string.IsNullOrWhiteSpace(name))
            return "Product name is required.";

        if (name.Trim().Length < 2)
            return "Product name must be at least 2 characters.";

        if (name.Trim().Length > 150)
            return "Product name is too long (max 150).";

        if (originalPrice <= 0)
            return "Original price must be greater than zero.";

        if (sellingPrice <= 0)
            return "Selling price must be greater than zero.";

        if (originalPrice > 10_000_000 || sellingPrice > 10_000_000)
            return "Price is unrealistically large.";

        if (stock < 0)
            return "Stock quantity cannot be negative.";

        if (stock > 1_000_000)
            return "Stock quantity is unrealistically large.";

        return null;
    }


    private static string? ValidateSaleInput(int quantity, decimal price, DateTime date)
    {
        if (quantity <= 0)
            return "Quantity must be greater than zero.";

        if (quantity > 100_000)
            return "Quantity is unrealistically large.";

        if (price <= 0)
            return "Sale price must be greater than zero.";

        if (price > 10_000_000)
            return "Sale price is unrealistically large.";

        if (date.Date > DateTime.Today)
            return "Sale date cannot be in the future.";

        if (date.Year < 2000)
            return "Sale date is too old.";

        return null;
    }


    private static string? ValidateFilter(ProductFilterDto f)
    {
        if (f.MinPrice.HasValue && f.MaxPrice.HasValue && f.MinPrice > f.MaxPrice)
            return "Min price cannot exceed max price.";

        if (f.MinCost.HasValue && f.MaxCost.HasValue && f.MinCost > f.MaxCost)
            return "Min cost cannot exceed max cost.";

        if (f.MinStock.HasValue && f.MaxStock.HasValue && f.MinStock > f.MaxStock)
            return "Min stock cannot exceed max stock.";

        if (f.LowStockThreshold < 0)
            return "Invalid low stock threshold.";

        return null;
    }
}