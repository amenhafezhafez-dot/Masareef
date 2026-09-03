using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Masareef.Entities.Models;

[Index("UserId", "ExpenseDate", Name = "IX_HomeExpenses_Date")]
[Index("UserId", Name = "IX_HomeExpenses_UserID")]
public partial class HomeExpense
{
    [Key]
    [Column("HomeExpenseID")]
    public int HomeExpenseId { get; set; }

    [Column("UserID")]
    public int UserId { get; set; }

    [Column("CategoryID")]
    public int CategoryId { get; set; }

    [Column(TypeName = "decimal(18, 2)")]
    public decimal Amount { get; set; }

    public DateTime ExpenseDate { get; set; }

    public byte PaymentMethod { get; set; }

    [StringLength(500)]
    public string? Description { get; set; }

    public DateTime CreatedAt { get; set; }

    [ForeignKey("CategoryId")]
    [InverseProperty("HomeExpenses")]
    public virtual Category Category { get; set; } = null!;

    [ForeignKey("UserId")]
    [InverseProperty("HomeExpenses")]
    public virtual User User { get; set; } = null!;
}
