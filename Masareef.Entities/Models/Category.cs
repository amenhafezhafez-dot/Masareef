using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Masareef.Entities.Models;

[Index("UserId", Name = "IX_Categories_UserID")]
public partial class Category
{
    [Key]
    [Column("CategoryID")]
    public int CategoryId { get; set; }

    [Column("UserID")]
    public int? UserId { get; set; }

    [StringLength(100)]
    public string CategoryName { get; set; } = null!;

    public byte CategoryScope { get; set; }

    public byte CategoryKind { get; set; }

    [StringLength(50)]
    public string? Icon { get; set; }

    [InverseProperty("Category")]
    public virtual ICollection<BusinessExpense> BusinessExpenses { get; set; } = new List<BusinessExpense>();

    [InverseProperty("Category")]
    public virtual ICollection<BusinessIncome> BusinessIncomes { get; set; } = new List<BusinessIncome>();

    [InverseProperty("Category")]
    public virtual ICollection<HomeExpense> HomeExpenses { get; set; } = new List<HomeExpense>();

    [InverseProperty("Category")]
    public virtual ICollection<HomeIncome> HomeIncomes { get; set; } = new List<HomeIncome>();

    [ForeignKey("UserId")]
    [InverseProperty("Categories")]
    public virtual User? User { get; set; }
}
