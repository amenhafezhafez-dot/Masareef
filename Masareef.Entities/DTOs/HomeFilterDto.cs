namespace Masareef.Entities.DTOs;

/// <summary>فلتر مرن لمصاريف ودخل البيت — كل الحقول اختيارية</summary>
public class HomeFilterDto
{
    public int? Year { get; set; }
    public int? Month { get; set; }
    public int? CategoryId { get; set; }

    public DateTime? DateFrom { get; set; }      // نطاق بدل سنة/شهر فقط
    public DateTime? DateTo { get; set; }

    public decimal? MinAmount { get; set; }
    public decimal? MaxAmount { get; set; }

    public byte? PaymentMethod { get; set; }     // للمصاريف فقط
    public string? SearchText { get; set; }      // بحث في الوصف/المصدر
}