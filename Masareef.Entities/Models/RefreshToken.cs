using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Masareef.Entities.Models;

public class RefreshToken
{
    [Key]
    public int Id { get; set; }

    public int UserId { get; set; }

    /// <summary>SHA-256 hash للتوكن — مبنخزنش التوكن الخام في الداتابيز أبداً</summary>
    [StringLength(64)]
    public string TokenHash { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public DateTime ExpiresAt { get; set; }

    public DateTime? RevokedAt { get; set; }

    /// <summary>لو اتعمله rotate، ده الـ hash بتاع التوكن الجديد اللي حل مكانه</summary>
    [StringLength(64)]
    public string? ReplacedByTokenHash { get; set; }

    [ForeignKey("UserId")]
    public virtual User User { get; set; } = null!;

    [NotMapped]
    public bool IsActive => RevokedAt is null && ExpiresAt > DateTime.UtcNow;
}
