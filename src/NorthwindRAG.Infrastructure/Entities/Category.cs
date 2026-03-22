using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NorthwindRAG.Infrastructure.Entities;

[Table("Categories")]
public class Category
{
    [Key]
    [Column("CategoryID")]
    public int CategoryId { get; set; }

    [Column("CategoryName")]
    public string CategoryName { get; set; } = string.Empty;

    [Column("Description")]
    public string? Description { get; set; }

    public ICollection<Product> Products { get; set; } = new List<Product>();
}
