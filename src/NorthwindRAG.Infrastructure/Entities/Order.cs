using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NorthwindRAG.Infrastructure.Entities;

[Table("Orders")]
public class Order
{
    [Key]
    [Column("OrderID")]
    public int OrderId { get; set; }

    [Column("CustomerID")]
    public string? CustomerId { get; set; }

    [Column("EmployeeID")]
    public int? EmployeeId { get; set; }

    [Column("OrderDate")]
    public DateTime? OrderDate { get; set; }

    [Column("RequiredDate")]
    public DateTime? RequiredDate { get; set; }

    [Column("ShippedDate")]
    public DateTime? ShippedDate { get; set; }

    [Column("ShipVia")]
    public int? ShipVia { get; set; }

    [Column("Freight", TypeName = "money")]
    public decimal? Freight { get; set; }

    [Column("ShipName")]
    public string? ShipName { get; set; }

    [Column("ShipAddress")]
    public string? ShipAddress { get; set; }

    [Column("ShipCity")]
    public string? ShipCity { get; set; }

    [Column("ShipRegion")]
    public string? ShipRegion { get; set; }

    [Column("ShipPostalCode")]
    public string? ShipPostalCode { get; set; }

    [Column("ShipCountry")]
    public string? ShipCountry { get; set; }

    public Customer? Customer { get; set; }
    public Employee? Employee { get; set; }
    public ICollection<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();
}
