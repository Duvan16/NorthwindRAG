using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NorthwindRAG.Infrastructure.Entities;

[Table("Products")]
public class Product
{
    [Key]
    [Column("ProductID")]
    public int ProductId { get; set; }

    [Column("ProductName")]
    public string ProductName { get; set; } = string.Empty;

    [Column("SupplierID")]
    public int? SupplierId { get; set; }

    [Column("CategoryID")]
    public int? CategoryId { get; set; }

    [Column("QuantityPerUnit")]
    public string? QuantityPerUnit { get; set; }

    [Column("UnitPrice", TypeName = "money")]
    public decimal? UnitPrice { get; set; }

    [Column("UnitsInStock")]
    public short? UnitsInStock { get; set; }

    [Column("UnitsOnOrder")]
    public short? UnitsOnOrder { get; set; }

    [Column("ReorderLevel")]
    public short? ReorderLevel { get; set; }

    [Column("Discontinued")]
    public bool Discontinued { get; set; }

    public Category? Category { get; set; }
    public Supplier? Supplier { get; set; }
    public ICollection<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();
}
