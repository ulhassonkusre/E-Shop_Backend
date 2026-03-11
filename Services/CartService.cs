using EcommerceBackend.Models;

namespace EcommerceBackend.Services;

public interface ICartService
{
    CartResponseDto GetCart(int userId);
    CartItemResponseDto? AddToCart(int userId, AddToCartDto dto);
    CartItemResponseDto? UpdateCartItem(int userId, int itemId, UpdateCartItemDto dto);
    bool RemoveFromCart(int userId, int itemId);
    bool ClearCart(int userId);
}

public class CartService : ICartService
{
    private readonly IProductService _productService;

    // In-memory storage
    private static readonly List<CartItem> CartItems = new();
    private static int _nextId = 1;

    public CartService(IProductService productService)
    {
        _productService = productService;
    }

    public CartResponseDto GetCart(int userId)
    {
        var items = CartItems
            .Where(ci => ci.UserId == userId)
            .Select(ci =>
            {
                var product = _productService.GetById(ci.ProductId);
                return new CartItemResponseDto
                {
                    Id = ci.Id,
                    ProductId = ci.ProductId,
                    ProductName = product?.Name ?? "Unknown",
                    ProductImage = product?.ImageUrl ?? "",
                    Price = product?.Price ?? 0,
                    Quantity = ci.Quantity
                };
            })
            .ToList();

        return new CartResponseDto
        {
            UserId = userId,
            Items = items
        };
    }

    public CartItemResponseDto? AddToCart(int userId, AddToCartDto dto)
    {
        var existingItem = CartItems.FirstOrDefault(ci => 
            ci.UserId == userId && ci.ProductId == dto.ProductId);

        if (existingItem != null)
        {
            existingItem.Quantity += dto.Quantity;
            return MapToResponse(existingItem);
        }

        var cartItem = new CartItem
        {
            Id = _nextId++,
            UserId = userId,
            ProductId = dto.ProductId,
            Quantity = dto.Quantity
        };

        CartItems.Add(cartItem);
        return MapToResponse(cartItem);
    }

    public CartItemResponseDto? UpdateCartItem(int userId, int itemId, UpdateCartItemDto dto)
    {
        var cartItem = CartItems.FirstOrDefault(ci => 
            ci.Id == itemId && ci.UserId == userId);

        if (cartItem == null)
            return null;

        cartItem.Quantity = dto.Quantity;
        return MapToResponse(cartItem);
    }

    public bool RemoveFromCart(int userId, int itemId)
    {
        var cartItem = CartItems.FirstOrDefault(ci => 
            ci.Id == itemId && ci.UserId == userId);

        if (cartItem == null)
            return false;

        CartItems.Remove(cartItem);
        return true;
    }

    public bool ClearCart(int userId)
    {
        var items = CartItems.Where(ci => ci.UserId == userId).ToList();
        foreach (var item in items)
        {
            CartItems.Remove(item);
        }
        return true;
    }

    private CartItemResponseDto MapToResponse(CartItem cartItem)
    {
        var product = _productService.GetById(cartItem.ProductId);
        return new CartItemResponseDto
        {
            Id = cartItem.Id,
            ProductId = cartItem.ProductId,
            ProductName = product?.Name ?? "Unknown",
            ProductImage = product?.ImageUrl ?? "",
            Price = product?.Price ?? 0,
            Quantity = cartItem.Quantity
        };
    }
}
