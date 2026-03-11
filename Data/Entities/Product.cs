using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EcommerceBackend.Data.Entities;

[Table("Products")]
public class Product
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }
    
    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;
    
    [Required]
    [Column(TypeName = "TEXT")]
    public string Description { get; set; } = string.Empty;
    
    [Required]
    [Column(TypeName = "DECIMAL(10,2)")]
    public decimal Price { get; set; }
    
    [Required]
    [MaxLength(500)]
    public string ImageUrl { get; set; } = string.Empty;
    
    public int Stock { get; set; }
    
    [Required]
    [MaxLength(100)]
    public string Category { get; set; } = string.Empty;
    
    [Column(TypeName = "DATETIME")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    [Column(TypeName = "DATETIME")]
    public DateTime? UpdatedAt { get; set; }
    
    // Navigation properties
    public ICollection<CartItem> CartItems { get; set; } = new List<CartItem>();
    public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
    public ICollection<WishlistItem> WishlistItems { get; set; } = new List<WishlistItem>();
}
