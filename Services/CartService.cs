using EcommerceBackend.Models;

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
    private readonly IProductService _productService;
    private readonly IRedisCacheService _cache;
    private readonly TimeSpan _cacheExpiry = TimeSpan.FromMinutes(15);

    // In-memory storage
    private static readonly List<CartItem> CartItems = new();
    private static int _nextId = 1;

    // Cache keys
    private const string CART_BY_USER_KEY = "cart:user:{0}";

    public CartService(IProductService productService, IRedisCacheService cache)
    {
        _productService = productService;
        _cache = cache;
    }

    // Synchronous methods for backward compatibility
    public CartResponseDto GetCart(int userId)
    {
        return GetCartAsync(userId).Result;
    }

    public CartItemResponseDto? AddToCart(int userId, AddToCartDto dto)
    {
        return AddToCartAsync(userId, dto).Result;
    }

    public CartItemResponseDto? UpdateCartItem(int userId, int itemId, UpdateCartItemDto dto)
    {
        return UpdateCartItemAsync(userId, itemId, dto).Result;
    }

    public bool RemoveFromCart(int userId, int itemId)
    {
        return RemoveFromCartAsync(userId, itemId).Result;
    }

    public bool ClearCart(int userId)
    {
        return ClearCartAsync(userId).Result;
    }

    // Async methods with Redis caching
    public async Task<CartResponseDto> GetCartAsync(int userId)
    {
        var cacheKey = string.Format(CART_BY_USER_KEY, userId);
        
        // Try to get from cache first
        var cachedCart = await _cache.GetAsync<CartResponseDto>(cacheKey);
        if (cachedCart != null)
        {
            return cachedCart;
        }

        // If not in cache, build cart from memory and cache it
        var items = await BuildCartItemsAsync(userId);
        var cart = new CartResponseDto
        {
            UserId = userId,
            Items = items
        };

        await _cache.SetAsync(cacheKey, cart, _cacheExpiry);
        return cart;
    }

    public async Task<CartItemResponseDto?> AddToCartAsync(int userId, AddToCartDto dto)
    {
        var existingItem = CartItems.FirstOrDefault(ci =>
            ci.UserId == userId && ci.ProductId == dto.ProductId);

        if (existingItem != null)
        {
            existingItem.Quantity += dto.Quantity;
            await InvalidateUserCartCacheAsync(userId);
            return await MapToResponseAsync(existingItem);
        }

        var cartItem = new CartItem
        {
            Id = _nextId++,
            UserId = userId,
            ProductId = dto.ProductId,
            Quantity = dto.Quantity
        };

        CartItems.Add(cartItem);
        await InvalidateUserCartCacheAsync(userId);
        return await MapToResponseAsync(cartItem);
    }

    public async Task<CartItemResponseDto?> UpdateCartItemAsync(int userId, int itemId, UpdateCartItemDto dto)
    {
        var cartItem = CartItems.FirstOrDefault(ci =>
            ci.Id == itemId && ci.UserId == userId);

        if (cartItem == null)
            return null;

        cartItem.Quantity = dto.Quantity;
        await InvalidateUserCartCacheAsync(userId);
        return await MapToResponseAsync(cartItem);
    }

    public async Task<bool> RemoveFromCartAsync(int userId, int itemId)
    {
        var cartItem = CartItems.FirstOrDefault(ci =>
            ci.Id == itemId && ci.UserId == userId);

        if (cartItem == null)
            return false;

        CartItems.Remove(cartItem);
        await InvalidateUserCartCacheAsync(userId);
        return true;
    }

    public async Task<bool> ClearCartAsync(int userId)
    {
        var items = CartItems.Where(ci => ci.UserId == userId).ToList();
        foreach (var item in items)
        {
            CartItems.Remove(item);
        }
        
        await InvalidateUserCartCacheAsync(userId);
        return true;
    }

    private async Task InvalidateUserCartCacheAsync(int userId)
    {
        var cacheKey = string.Format(CART_BY_USER_KEY, userId);
        await _cache.RemoveAsync(cacheKey);
    }

    private async Task<List<CartItemResponseDto>> BuildCartItemsAsync(int userId)
    {
        var items = CartItems
            .Where(ci => ci.UserId == userId)
            .ToList();

        var cartItems = new List<CartItemResponseDto>();
        foreach (var ci in items)
        {
            var product = await _productService.GetByIdAsync(ci.ProductId);
            cartItems.Add(new CartItemResponseDto
            {
                Id = ci.Id,
                ProductId = ci.ProductId,
                ProductName = product?.Name ?? "Unknown",
                ProductImage = product?.ImageUrl ?? "",
                Price = product?.Price ?? 0,
                Quantity = ci.Quantity
            });
        }

        return cartItems;
    }

    private async Task<CartItemResponseDto?> MapToResponseAsync(CartItem cartItem)
    {
        var product = await _productService.GetByIdAsync(cartItem.ProductId);
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
