using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Masareef.Entities.Models;

[Index("PaymentDate", Name = "IX_DebtPayments_Date")]
[Index("DebtId", Name = "IX_DebtPayments_DebtID")]
public partial class DebtPayment
{
    [Key]
    [Column("PaymentID")]
    public int PaymentId { get; set; }

    [Column("DebtID")]
    public int DebtId { get; set; }

    [Column(TypeName = "decimal(18, 2)")]
    public decimal PaidAmount { get; set; }

    public DateTime PaymentDate { get; set; }

    public byte PaymentMethod { get; set; }

    [StringLength(300)]
    public string? Note { get; set; }

    public DateTime CreatedAt { get; set; }

    [ForeignKey("DebtId")]
    [InverseProperty("DebtPayments")]
    public virtual Debt Debt { get; set; } = null!;
}
