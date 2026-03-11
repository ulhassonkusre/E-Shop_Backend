using EcommerceBackend.Data;
using EcommerceBackend.Data.Entities;
using EcommerceBackend.Models;
using Microsoft.EntityFrameworkCore;

namespace EcommerceBackend.Services;

public interface ICartService
{
    Task<CartResponseDto> GetCartAsync(int userId);
    Task<CartItemResponseDto?> AddToCartAsync(int userId, AddToCartDto dto);
    Task<CartItemResponseDto?> UpdateCartItemAsync(int userId, int itemId, UpdateCartItemDto dto);
    Task<bool> RemoveFromCartAsync(int userId, int itemId);
    Task<bool> ClearCartAsync(int userId);
    CartResponseDto GetCart(int userId);
    CartItemResponseDto? AddToCart(int userId, AddToCartDto dto);
    CartItemResponseDto? UpdateCartItem(int userId, int itemId, UpdateCartItemDto dto);
    bool RemoveFromCart(int userId, int itemId);
    bool ClearCart(int userId);
}

public class CartService : ICartService
{
    private readonly EcommerceDbContext _context;

    public CartService(EcommerceDbContext context)
    {
        _context = context;
    }

    // Synchronous methods for backward compatibility
    public CartResponseDto GetCart(int userId) => GetCartAsync(userId).Result;
    public CartItemResponseDto? AddToCart(int userId, AddToCartDto dto) => AddToCartAsync(userId, dto).Result;
    public CartItemResponseDto? UpdateCartItem(int userId, int itemId, UpdateCartItemDto dto) => UpdateCartItemAsync(userId, itemId, dto).Result;
    public bool RemoveFromCart(int userId, int itemId) => RemoveFromCartAsync(userId, itemId).Result;
    public bool ClearCart(int userId) => ClearCartAsync(userId).Result;

    // Async methods with database
    public async Task<CartResponseDto> GetCartAsync(int userId)
    {
        var cartItems = await _context.CartItems
            .Where(ci => ci.UserId == userId)
            .Include(ci => ci.Product)
            .ToListAsync();

        var items = cartItems.Select(ci => new CartItemResponseDto
        {
            Id = ci.Id,
            ProductId = ci.ProductId,
            ProductName = ci.Product?.Name ?? "Unknown",
            ProductImage = ci.Product?.ImageUrl ?? "",
            Price = ci.Product?.Price ?? 0,
            Quantity = ci.Quantity
        }).ToList();

        return new CartResponseDto
        {
            UserId = userId,
            Items = items
        };
    }

    public async Task<CartItemResponseDto?> AddToCartAsync(int userId, AddToCartDto dto)
    {
        var existingItem = await _context.CartItems
            .FirstOrDefaultAsync(ci => ci.UserId == userId && ci.ProductId == dto.ProductId);

        if (existingItem != null)
        {
            existingItem.Quantity += dto.Quantity;
            await _context.SaveChangesAsync();
            return await MapToResponseAsync(existingItem);
        }

        var cartItem = new CartItem
        {
            UserId = userId,
            ProductId = dto.ProductId,
            Quantity = dto.Quantity,
            CreatedAt = DateTime.UtcNow
        };

        _context.CartItems.Add(cartItem);
        await _context.SaveChangesAsync();
        return await MapToResponseAsync(cartItem);
    }

    public async Task<CartItemResponseDto?> UpdateCartItemAsync(int userId, int itemId, UpdateCartItemDto dto)
    {
        var cartItem = await _context.CartItems
            .FirstOrDefaultAsync(ci => ci.Id == itemId && ci.UserId == userId);

        if (cartItem == null)
            return null;

        cartItem.Quantity = dto.Quantity;
        await _context.SaveChangesAsync();
        return await MapToResponseAsync(cartItem);
    }

    public async Task<bool> RemoveFromCartAsync(int userId, int itemId)
    {
        var cartItem = await _context.CartItems
            .FirstOrDefaultAsync(ci => ci.Id == itemId && ci.UserId == userId);

        if (cartItem == null)
            return false;

        _context.CartItems.Remove(cartItem);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ClearCartAsync(int userId)
    {
        var items = await _context.CartItems
            .Where(ci => ci.UserId == userId)
            .ToListAsync();
        
        _context.CartItems.RemoveRange(items);
        await _context.SaveChangesAsync();
        return true;
    }

    private async Task<CartItemResponseDto?> MapToResponseAsync(CartItem cartItem)
    {
        var product = await _context.Products.FindAsync(cartItem.ProductId);
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
