using EcommerceBackend.Models;
using EcommerceBackend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EcommerceBackend.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class OrdersController : ControllerBase
{
    private readonly IOrderService _orderService;
    private readonly IAuthService _authService;

    public OrdersController(IOrderService orderService, IAuthService authService)
    {
        _orderService = orderService;
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
    public async Task<ActionResult<List<OrderDto>>> GetUserOrders()
    {
        var userId = GetUserId();
        if (userId == 0)
            return Unauthorized();

        var orders = await _orderService.GetUserOrdersAsync(userId);
        return Ok(orders);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<OrderDto>> GetOrderById(int id)
    {
        var userId = GetUserId();
        if (userId == 0)
            return Unauthorized();

        var order = await _orderService.GetOrderByIdAsync(id);
        if (order == null)
            return NotFound();

        if (order.UserId != userId)
            return Forbid();

        return Ok(order);
    }

    [HttpPost]
    public async Task<ActionResult<OrderDto>> CreateOrder([FromBody] CreateOrderDto dto)
    {
        var userId = GetUserId();
        if (userId == 0)
            return Unauthorized();

        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            var order = await _orderService.CreateOrderAsync(userId, dto);
            return CreatedAtAction(nameof(GetOrderById), new { id = order.Id }, order);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPut("{id}/cancel")]
    public async Task<ActionResult> CancelOrder(int id)
    {
        var userId = GetUserId();
        if (userId == 0)
            return Unauthorized();

        var order = await _orderService.GetOrderByIdAsync(id);
        if (order == null)
            return NotFound();

        if (order.UserId != userId)
            return Forbid();

        var success = await _orderService.CancelOrderAsync(id);
        if (!success)
            return BadRequest("Cannot cancel this order");

        return NoContent();
    }

    [HttpGet("admin/all")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<List<OrderDto>>> GetAllOrders()
    {
        var orders = await _orderService.GetAllOrdersAsync();
        return Ok(orders);
    }
}
