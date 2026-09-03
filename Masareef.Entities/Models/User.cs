using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Masareef.Entities.Models;

[Index("Email", Name = "UQ_Users_Email", IsUnique = true)]
public partial class User
{
    [Key]
    [Column("UserID")]
    public int UserId { get; set; }

    [StringLength(100)]
    public string Name { get; set; } = null!;

    [StringLength(150)]
    public string Email { get; set; } = null!;

    [StringLength(255)]
    public string PasswordHash { get; set; } = null!;

    [StringLength(20)]
    public string? PhoneNumber { get; set; }

    public DateTime CreatedAt { get; set; }

    public bool IsActive { get; set; }

    [InverseProperty("User")]
    public virtual ICollection<Business> Businesses { get; set; } = new List<Business>();

    [InverseProperty("User")]
    public virtual ICollection<Category> Categories { get; set; } = new List<Category>();

    [InverseProperty("User")]
    public virtual ICollection<Debt> Debts { get; set; } = new List<Debt>();

    [InverseProperty("User")]
    public virtual ICollection<HomeExpense> HomeExpenses { get; set; } = new List<HomeExpense>();

    [InverseProperty("User")]
    public virtual ICollection<HomeIncome> HomeIncomes { get; set; } = new List<HomeIncome>();
}
