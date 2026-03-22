using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace NorthwindRAG.Infrastructure.Entities;

[Table("Order Details")]
[PrimaryKey(nameof(OrderId), nameof(ProductId))]
public class OrderDetail
{
    [Column("OrderID")]
    public int OrderId { get; set; }

    [Column("ProductID")]
    public int ProductId { get; set; }

    [Column("UnitPrice", TypeName = "money")]
    public decimal UnitPrice { get; set; }

    [Column("Quantity")]
    public short Quantity { get; set; }

    [Column("Discount")]
    public float Discount { get; set; }

    public Order? Order { get; set; }
    public Product? Product { get; set; }
}
