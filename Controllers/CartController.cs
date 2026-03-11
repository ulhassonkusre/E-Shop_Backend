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
    public ActionResult<CartResponseDto> GetCart()
    {
        var userId = GetUserId();
        if (userId == 0)
            return Unauthorized();

        var cart = _cartService.GetCart(userId);
        return Ok(cart);
    }

    [HttpPost("items")]
    public ActionResult<CartItemResponseDto> AddToCart([FromBody] AddToCartDto dto)
    {
        var userId = GetUserId();
        if (userId == 0)
            return Unauthorized();

        var item = _cartService.AddToCart(userId, dto);
        if (item == null)
            return BadRequest();

        return Ok(item);
    }

    [HttpPut("items/{itemId}")]
    public ActionResult<CartItemResponseDto> UpdateCartItem(int itemId, [FromBody] UpdateCartItemDto dto)
    {
        var userId = GetUserId();
        if (userId == 0)
            return Unauthorized();

        var item = _cartService.UpdateCartItem(userId, itemId, dto);
        if (item == null)
            return NotFound();

        return Ok(item);
    }

    [HttpDelete("items/{itemId}")]
    public ActionResult RemoveFromCart(int itemId)
    {
        var userId = GetUserId();
        if (userId == 0)
            return Unauthorized();

        if (!_cartService.RemoveFromCart(userId, itemId))
            return NotFound();

        return NoContent();
    }

    [HttpDelete("clear")]
    public ActionResult ClearCart()
    {
        var userId = GetUserId();
        if (userId == 0)
            return Unauthorized();

        _cartService.ClearCart(userId);
        return NoContent();
    }
}
