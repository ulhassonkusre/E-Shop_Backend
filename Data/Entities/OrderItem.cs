using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EcommerceBackend.Data.Entities;

[Table("OrderItems")]
public class OrderItem
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }
    
    public int OrderId { get; set; }
    
    public int ProductId { get; set; }
    
    [Required]
    [MaxLength(200)]
    public string ProductName { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(500)]
    public string ProductImage { get; set; } = string.Empty;
    
    [Required]
    [Column(TypeName = "DECIMAL(10,2)")]
    public decimal Price { get; set; }
    
    public int Quantity { get; set; } = 1;
    
    [Column(TypeName = "DATETIME")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    // Navigation properties
    [ForeignKey("OrderId")]
    public Order Order { get; set; } = null!;
    
    [ForeignKey("ProductId")]
    public Product Product { get; set; } = null!;
}
