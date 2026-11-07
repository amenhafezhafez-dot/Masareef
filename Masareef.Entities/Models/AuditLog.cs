using System.ComponentModel.DataAnnotations;

namespace Masareef.Entities.Models;

public class AuditLog
{
    [Key]
    public int Id { get; set; }

    /// <summary>NULL لو الحدث حصل قبل ما نعرف هوية اليوزر (مثلاً login فاشل بإيميل مش موجود)</summary>
    public int? UserId { get; set; }

    /// <summary>"Auth.LoginFailed", "Debt.WriteOff", "Business.Deactivate" ...</summary>
    [StringLength(100)]
    public string Action { get; set; } = null!;

    [StringLength(50)]
    public string? EntityType { get; set; }

    public int? EntityId { get; set; }

    public string? Details { get; set; }

    [StringLength(45)]
    public string? IpAddress { get; set; }

    public bool Success { get; set; } = true;

    public DateTime CreatedAt { get; set; }
}
