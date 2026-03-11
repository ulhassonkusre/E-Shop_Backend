using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EcommerceBackend.Data.Entities;

public enum OrderStatus
{
    Processing,
    Shipped,
    Delivered,
    Cancelled
}

[Table("Orders")]
public class Order
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }
    
    [Required]
    [MaxLength(50)]
    public string OrderNumber { get; set; } = string.Empty;
    
    public int UserId { get; set; }
    
    [Required]
    [Column(TypeName = "DECIMAL(10,2)")]
    public decimal TotalAmount { get; set; }
    
    [Required]
    [MaxLength(50)]
    public string Status { get; set; } = OrderStatus.Processing.ToString();
    
    [Required]
    [MaxLength(200)]
    public string ShippingFullName { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(200)]
    public string ShippingEmail { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(50)]
    public string ShippingPhone { get; set; } = string.Empty;
    
    [Required]
    [Column(TypeName = "TEXT")]
    public string ShippingAddress { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(100)]
    public string ShippingCity { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(100)]
    public string ShippingState { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(20)]
    public string ShippingZipCode { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(100)]
    public string ShippingCountry { get; set; } = string.Empty;
    
    [Column(TypeName = "DATETIME")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    // Navigation properties
    [ForeignKey("UserId")]
    public User User { get; set; } = null!;
    
    public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
}
