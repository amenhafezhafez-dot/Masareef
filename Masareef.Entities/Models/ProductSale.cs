using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Masareef.Entities.Models;

[Index("ProductId", Name = "IX_ProductSales_ProductID")]
[Index("SaleDate", Name = "IX_ProductSales_SaleDate")]
public partial class ProductSale
{
    [Key]
    [Column("SaleID")]
    public int SaleId { get; set; }

    [Column("ProductID")]
    public int ProductId { get; set; }

    public int QuantitySold { get; set; }

    [Column(TypeName = "decimal(18, 2)")]
    public decimal SalePrice { get; set; }

    public DateTime SaleDate { get; set; }

    [StringLength(100)]
    public string? CustomerName { get; set; }

    [StringLength(300)]
    public string? Note { get; set; }

    public DateTime CreatedAt { get; set; }

    [ForeignKey("ProductId")]
    [InverseProperty("ProductSales")]
    public virtual Product Product { get; set; } = null!;
}