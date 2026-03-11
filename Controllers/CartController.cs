using EcommerceBackend.Models;
using EcommerceBackend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EcommerceBackend.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CartController : ControllerBase
{
    private readonly ICartService _cartService;
    private readonly IAuthService _authService;

    public CartController(ICartService cartService, IAuthService authService)
    {
        _cartService = cartService;
        _authService = authService;
    }

    private int GetUserId()
    {
        var authHeader = Request.Headers["Authorization"].ToString();
        if (string.IsNullOrEmpty(authHeader) || !authHeader.StartsWith("Bearer "))
            return 0;

        var token = authHeader.Substring("Bearer ".Length).Trim();
        return _authService.GetUserIdFromToken(token) ?? 0;
    }

    [HttpGet]
    public async Task<ActionResult<CartResponseDto>> GetCart()
    {
        var userId = GetUserId();
        if (userId == 0)
            return Unauthorized();

        var cart = await _cartService.GetCartAsync(userId);
        return Ok(cart);
    }

    [HttpPost("items")]
    public async Task<ActionResult<CartItemResponseDto>> AddToCart([FromBody] AddToCartDto dto)
    {
        var userId = GetUserId();
        if (userId == 0)
            return Unauthorized();

        var item = await _cartService.AddToCartAsync(userId, dto);
        if (item == null)
            return BadRequest();

        return Ok(item);
    }

    [HttpPut("items/{itemId}")]
    public async Task<ActionResult<CartItemResponseDto>> UpdateCartItem(int itemId, [FromBody] UpdateCartItemDto dto)
    {
        var userId = GetUserId();
        if (userId == 0)
            return Unauthorized();

        var item = await _cartService.UpdateCartItemAsync(userId, itemId, dto);
        if (item == null)
            return NotFound();

        return Ok(item);
    }

    [HttpDelete("items/{itemId}")]
    public async Task<ActionResult> RemoveFromCart(int itemId)
    {
        var userId = GetUserId();
        if (userId == 0)
            return Unauthorized();

        if (!await _cartService.RemoveFromCartAsync(userId, itemId))
            return NotFound();

        return NoContent();
    }

    [HttpDelete("clear")]
    public async Task<ActionResult> ClearCart()
    {
        var userId = GetUserId();
        if (userId == 0)
            return Unauthorized();

        await _cartService.ClearCartAsync(userId);
        return NoContent();
    }
}
