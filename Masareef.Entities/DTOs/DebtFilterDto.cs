namespace Masareef.Entities.DTOs;

/// <summary>فلتر مرن للبحث في الديون — كل الحقول اختيارية</summary>
public class DebtFilterDto
{
    public byte? Status { get; set; }          // 1..4
    public byte? Direction { get; set; }       // 1=عليّ 2=ليّ
    public string? PersonName { get; set; }    // بحث جزئي
    public bool? OnlyOverdue { get; set; }     // المتأخرة فقط
    public bool? OnlyNotDone { get; set; }     // المفتوحة والجزئية

    public DateTime? DueFrom { get; set; }     // نطاق بدل مساواة
    public DateTime? DueTo { get; set; }
    public DateTime? DebtFrom { get; set; }
    public DateTime? DebtTo { get; set; }

    public decimal? MinAmount { get; set; }    // نطاق بدل مساواة
    public decimal? MaxAmount { get; set; }
}