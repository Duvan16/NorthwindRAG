using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NorthwindRAG.Infrastructure.Entities;

[Table("Employees")]
public class Employee
{
    [Key]
    [Column("EmployeeID")]
    public int EmployeeId { get; set; }

    [Column("LastName")]
    public string LastName { get; set; } = string.Empty;

    [Column("FirstName")]
    public string FirstName { get; set; } = string.Empty;

    [Column("Title")]
    public string? Title { get; set; }

    [Column("TitleOfCourtesy")]
    public string? TitleOfCourtesy { get; set; }

    [Column("BirthDate")]
    public DateTime? BirthDate { get; set; }

    [Column("HireDate")]
    public DateTime? HireDate { get; set; }

    [Column("Address")]
    public string? Address { get; set; }

    [Column("City")]
    public string? City { get; set; }

    [Column("Region")]
    public string? Region { get; set; }

    [Column("PostalCode")]
    public string? PostalCode { get; set; }

    [Column("Country")]
    public string? Country { get; set; }

    [Column("HomePhone")]
    public string? HomePhone { get; set; }

    [Column("Notes")]
    public string? Notes { get; set; }

    [Column("ReportsTo")]
    public int? ReportsTo { get; set; }

    public ICollection<Order> Orders { get; set; } = new List<Order>();
}
