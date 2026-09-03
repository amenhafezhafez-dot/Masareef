using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Masareef.Entities.Models;

[Keyless]
public partial class VwDebtSummary
{
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
    public decimal OriginalAmount { get; set; }

    [Column(TypeName = "decimal(38, 2)")]
    public decimal TotalPaid { get; set; }

    [Column(TypeName = "decimal(38, 2)")]
    public decimal? Outstanding { get; set; }

    public DateOnly DebtDate { get; set; }

    public DateOnly? DueDate { get; set; }

    public byte Status { get; set; }

    public int IsOverdue { get; set; }

    public int? PaymentsCount { get; set; }
}
