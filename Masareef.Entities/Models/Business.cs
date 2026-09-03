using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Masareef.Entities.Models;

[Index("UserId", Name = "IX_Businesses_UserID")]
public partial class Business
{
    [Key]
    [Column("BusinessID")]
    public int BusinessId { get; set; }

    [Column("UserID")]
    public int UserId { get; set; }

    [StringLength(100)]
    public string BusinessName { get; set; } = null!;

    [StringLength(500)]
    public string? Description { get; set; }

    [StringLength(3)]
    public string Currency { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public bool IsActive { get; set; }

    [InverseProperty("Business")]
    public virtual ICollection<BusinessExpense> BusinessExpenses { get; set; } = new List<BusinessExpense>();

    [InverseProperty("Business")]
    public virtual ICollection<BusinessIncome> BusinessIncomes { get; set; } = new List<BusinessIncome>();

    [InverseProperty("Business")]
    public virtual ICollection<Debt> Debts { get; set; } = new List<Debt>();

    [InverseProperty("Business")]
    public virtual ICollection<Product> Products { get; set; } = new List<Product>();

    [ForeignKey("UserId")]
    [InverseProperty("Businesses")]
    public virtual User User { get; set; } = null!;
}
