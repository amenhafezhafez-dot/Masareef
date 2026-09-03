using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Masareef.Entities.Models;

[Index("BusinessId", Name = "IX_BusinessExpenses_BusinessID")]
[Index("CategoryId", Name = "IX_BusinessExpenses_CategoryID")]
[Index("BusinessId", "ExpenseDate", Name = "IX_BusinessExpenses_Date")]
public partial class BusinessExpense
{
    [Key]
    [Column("ExpenseID")]
    public int ExpenseId { get; set; }

    [Column("BusinessID")]
    public int BusinessId { get; set; }

    [Column("CategoryID")]
    public int CategoryId { get; set; }

    [Column(TypeName = "decimal(18, 2)")]
    public decimal Amount { get; set; }

    public DateTime ExpenseDate { get; set; }

    public byte PaymentMethod { get; set; }

    [StringLength(500)]
    public string? Description { get; set; }

    public DateTime CreatedAt { get; set; }

    [ForeignKey("BusinessId")]
    [InverseProperty("BusinessExpenses")]
    public virtual Business Business { get; set; } = null!;

    [ForeignKey("CategoryId")]
    [InverseProperty("BusinessExpenses")]
    public virtual Category Category { get; set; } = null!;
}
