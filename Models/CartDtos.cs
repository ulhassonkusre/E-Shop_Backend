namespace EcommerceBackend.Models;

public class AddToCartDto
{
    public int ProductId { get; set; }
    public int Quantity { get; set; } = 1;
}

public class UpdateCartItemDto
{
    public int Quantity { get; set; }
}

public class CartItemResponseDto
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string ProductImage { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int Quantity { get; set; }
    public decimal TotalPrice => Price * Quantity;
}

public class CartResponseDto
{
    public int UserId { get; set; }
    public List<CartItemResponseDto> Items { get; set; } = new();
    public decimal TotalAmount => Items.Sum(i => i.TotalPrice);
    public int TotalItems => Items.Sum(i => i.Quantity);
}
