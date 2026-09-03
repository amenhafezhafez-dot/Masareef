using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Masareef.Entities.Models;

[Index("BusinessId", Name = "IX_Products_BusinessID")]
public partial class Product
{
    [Key]
    [Column("ProductID")]
    public int ProductId { get; set; }

    [Column("BusinessID")]
    public int BusinessId { get; set; }

    [StringLength(150)]
    public string ProductName { get; set; } = null!;

    [StringLength(50)]
    public string? ProductCode { get; set; }

    [Column(TypeName = "decimal(18, 2)")]
    public decimal OriginalPrice { get; set; }

    [Column(TypeName = "decimal(18, 2)")]
    public decimal SellingPrice { get; set; }

    public int StockQuantity { get; set; }

    [StringLength(500)]
    public string? Description { get; set; }

    public bool IsActive { get; set; }

    public DateTime CreatedAt { get; set; }

    [ForeignKey("BusinessId")]
    [InverseProperty("Products")]
    public virtual Business Business { get; set; } = null!;

    [InverseProperty("Product")]
    public virtual ICollection<ProductSale> ProductSales { get; set; } = new List<ProductSale>();
}