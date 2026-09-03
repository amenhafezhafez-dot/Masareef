namespace Masareef.Entities.DTOs;

/// <summary>فلتر البحث في المحلات</summary>
public class BusinessFilterDto
{
    public string? Name { get; set; }            // بحث جزئي بالاسم
    public string? Currency { get; set; }
    public bool? IsActive { get; set; }          // null = الكل
    public DateTime? CreatedFrom { get; set; }
    public DateTime? CreatedTo { get; set; }
}

/// <summary>فلتر البحث في عمليات المحل (دخل/مصاريف)</summary>
public class BusinessTransactionFilterDto
{
    public int? Year { get; set; }
    public int? Month { get; set; }
    public int? CategoryId { get; set; }

    public DateTime? DateFrom { get; set; }
    public DateTime? DateTo { get; set; }

    public decimal? MinAmount { get; set; }
    public decimal? MaxAmount { get; set; }

    public byte? PaymentMethod { get; set; }     // للمصاريف
    public string? SearchText { get; set; }      // في الوصف/المصدر
}