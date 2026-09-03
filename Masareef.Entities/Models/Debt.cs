using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Masareef.Entities.Models;

[Index("BusinessId", Name = "IX_Debts_BusinessID")]
[Index("UserId", "Status", Name = "IX_Debts_Status")]
[Index("UserId", Name = "IX_Debts_UserID")]
public partial class Debt
{
    [Key]
    [Column("DebtID")]
    public int DebtId { get; set; }

    [Column("UserID")]
    public int UserId { get; set; }

    [Column("BusinessID")]
    public int? BusinessId { get; set; }

    [StringLength(100)]
    public string PersonName { get; set; } = null!;

    [StringLength(20)]
    public string? PersonPhone { get; set; }

    public byte Direction { get; set; }

    [Column(TypeName = "decimal(18, 2)")]
    public decimal Amount { get; set; }

    public DateTime DebtDate { get; set; }

    public DateTime? DueDate { get; set; }

    public byte Status { get; set; }

    [StringLength(500)]
    public string? Description { get; set; }

    public DateTime CreatedAt { get; set; }

    [StringLength(500)]
    public string? WriteOffReason { get; set; }

    public DateTime? WriteOffDate { get; set; }

    [Timestamp]
    public byte[] RowVersion { get; set; } = null!;


    [ForeignKey("BusinessId")]
    [InverseProperty("Debts")]
    public virtual Business? Business { get; set; }

    [InverseProperty("Debt")]
    public virtual ICollection<DebtPayment> DebtPayments { get; set; } = new List<DebtPayment>();

    [ForeignKey("UserId")]
    [InverseProperty("Debts")]
    public virtual User User { get; set; } = null!;
}
