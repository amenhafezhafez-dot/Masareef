using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Masareef.Entities.Models;

[Index("UserId", Name = "IX_HomeIncomes_UserID")]
public partial class HomeIncome
{
    [Key]
    [Column("HomeIncomeID")]
    public int HomeIncomeId { get; set; }

    [Column("UserID")]
    public int UserId { get; set; }

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

    [ForeignKey("CategoryId")]
    [InverseProperty("HomeIncomes")]
    public virtual Category Category { get; set; } = null!;

    [ForeignKey("UserId")]
    [InverseProperty("HomeIncomes")]
    public virtual User User { get; set; } = null!;
}
