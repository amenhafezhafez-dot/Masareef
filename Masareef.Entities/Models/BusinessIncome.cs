using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Masareef.Entities.Models;

[Index("BusinessId", Name = "IX_BusinessIncomes_BusinessID")]
[Index("BusinessId", "IncomeDate", Name = "IX_BusinessIncomes_Date")]
public partial class BusinessIncome
{
    [Key]
    [Column("IncomeID")]
    public int IncomeId { get; set; }

    [Column("BusinessID")]
    public int BusinessId { get; set; }

    [Column("CategoryID")]
    public int CategoryId { get; set; }

    [Column(TypeName = "decimal(18, 2)")]
    public decimal Amount { get; set; }

    public DateTime IncomeDate { get; set; }

    [StringLength(100)]
    public string? Source { get; set; }

    [StringLength(500)]
    public string? Description { get; set; }

    public DateTime CreatedAt { get; set; }

    [ForeignKey("BusinessId")]
    [InverseProperty("BusinessIncomes")]
    public virtual Business Business { get; set; } = null!;

    [ForeignKey("CategoryId")]
    [InverseProperty("BusinessIncomes")]
    public virtual Category Category { get; set; } = null!;
}
