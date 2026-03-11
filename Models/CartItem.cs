namespace EcommerceBackend.Models;

public class CartItem
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public int ProductId { get; set; }
    public int Quantity { get; set; }
    public DateTime AddedAt { get; set; } = DateTime.UtcNow;
    
    // Navigation property (not persisted in memory)
    public Product? Product { get; set; }
}
