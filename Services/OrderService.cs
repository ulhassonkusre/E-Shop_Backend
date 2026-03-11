using EcommerceBackend.Data;
using EcommerceBackend.Data.Entities;
using EcommerceBackend.Models;
using Microsoft.EntityFrameworkCore;

namespace EcommerceBackend.Services;

public interface IOrderService
{
    Task<OrderDto> CreateOrderAsync(int userId, CreateOrderDto dto);
    Task<List<OrderDto>> GetUserOrdersAsync(int userId);
    Task<OrderDto?> GetOrderByIdAsync(int orderId);
    Task<bool> CancelOrderAsync(int orderId);
    Task<List<OrderDto>> GetAllOrdersAsync();
}

public class OrderService : IOrderService
{
    private readonly EcommerceDbContext _context;

    public OrderService(EcommerceDbContext context)
    {
        _context = context;
    }

    public async Task<OrderDto> CreateOrderAsync(int userId, CreateOrderDto dto)
    {
        // Generate unique order number
        var orderNumber = $"ORD-{DateTime.UtcNow:yyyyMMddHHmmss}-{Guid.NewGuid().ToString().Substring(0, 4).ToUpper()}";

        // Calculate total from cart items
        var cartItems = await _context.CartItems
            .Where(ci => ci.UserId == userId)
            .Include(ci => ci.Product)
            .ToListAsync();

        if (!cartItems.Any())
        {
            throw new InvalidOperationException("Cart is empty");
        }

        var totalAmount = cartItems.Sum(ci => ci.Product!.Price * ci.Quantity);

        // Create order
        var order = new Order
        {
            OrderNumber = orderNumber,
            UserId = userId,
            TotalAmount = totalAmount,
            Status = OrderStatus.Processing.ToString(),
            ShippingFullName = dto.ShippingFullName,
            ShippingEmail = dto.ShippingEmail,
            ShippingPhone = dto.ShippingPhone,
            ShippingAddress = dto.ShippingAddress,
            ShippingCity = dto.ShippingCity,
            ShippingState = dto.ShippingState,
            ShippingZipCode = dto.ShippingZipCode,
            ShippingCountry = dto.ShippingCountry,
            CreatedAt = DateTime.UtcNow
        };

        _context.Orders.Add(order);
        await _context.SaveChangesAsync();

        // Create order items
        var orderItems = cartItems.Select(ci => new OrderItem
        {
            OrderId = order.Id,
            ProductId = ci.ProductId,
            ProductName = ci.Product!.Name,
            ProductImage = ci.Product!.ImageUrl,
            Price = ci.Product!.Price,
            Quantity = ci.Quantity,
            CreatedAt = DateTime.UtcNow
        }).ToList();

        _context.OrderItems.AddRange(orderItems);

        // Update product stock
        foreach (var cartItem in cartItems)
        {
            if (cartItem.Product != null)
            {
                cartItem.Product.Stock -= cartItem.Quantity;
            }
        }

        // Clear cart
        _context.CartItems.RemoveRange(cartItems);

        await _context.SaveChangesAsync();

        return MapToDto(order, orderItems);
    }

    public async Task<List<OrderDto>> GetUserOrdersAsync(int userId)
    {
        var orders = await _context.Orders
            .Where(o => o.UserId == userId)
            .OrderByDescending(o => o.CreatedAt)
            .Include(o => o.OrderItems)
            .ToListAsync();

        return orders.Select(o => MapToDto(o, o.OrderItems.ToList())).ToList();
    }

    public async Task<OrderDto?> GetOrderByIdAsync(int orderId)
    {
        var order = await _context.Orders
            .Include(o => o.OrderItems)
            .FirstOrDefaultAsync(o => o.Id == orderId);

        if (order == null)
            return null;

        return MapToDto(order, order.OrderItems.ToList());
    }

    public async Task<bool> CancelOrderAsync(int orderId)
    {
        var order = await _context.Orders.FindAsync(orderId);
        if (order == null)
            return false;

        if (order.Status != OrderStatus.Processing.ToString())
        {
            return false; // Can only cancel processing orders
        }

        order.Status = OrderStatus.Cancelled.ToString();
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<List<OrderDto>> GetAllOrdersAsync()
    {
        var orders = await _context.Orders
            .OrderByDescending(o => o.CreatedAt)
            .Include(o => o.OrderItems)
            .ToListAsync();

        return orders.Select(o => MapToDto(o, o.OrderItems.ToList())).ToList();
    }

    private OrderDto MapToDto(Order order, List<OrderItem> orderItems)
    {
        return new OrderDto
        {
            Id = order.Id,
            OrderNumber = order.OrderNumber,
            UserId = order.UserId,
            TotalAmount = order.TotalAmount,
            Status = order.Status,
            ShippingFullName = order.ShippingFullName,
            ShippingEmail = order.ShippingEmail,
            ShippingPhone = order.ShippingPhone,
            ShippingAddress = order.ShippingAddress,
            ShippingCity = order.ShippingCity,
            ShippingState = order.ShippingState,
            ShippingZipCode = order.ShippingZipCode,
            ShippingCountry = order.ShippingCountry,
            CreatedAt = order.CreatedAt,
            Items = orderItems.Select(oi => new OrderItemDto
            {
                Id = oi.Id,
                ProductId = oi.ProductId,
                ProductName = oi.ProductName,
                ProductImage = oi.ProductImage,
                Price = oi.Price,
                Quantity = oi.Quantity
            }).ToList()
        };
    }
}

// DTOs for Orders
public class OrderDto
{
    public int Id { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public int UserId { get; set; }
    public decimal TotalAmount { get; set; }
    public string Status { get; set; } = string.Empty;
    public string ShippingFullName { get; set; } = string.Empty;
    public string ShippingEmail { get; set; } = string.Empty;
    public string ShippingPhone { get; set; } = string.Empty;
    public string ShippingAddress { get; set; } = string.Empty;
    public string ShippingCity { get; set; } = string.Empty;
    public string ShippingState { get; set; } = string.Empty;
    public string ShippingZipCode { get; set; } = string.Empty;
    public string ShippingCountry { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public List<OrderItemDto> Items { get; set; } = new();
}

public class OrderItemDto
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string ProductImage { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int Quantity { get; set; }
}

public class CreateOrderDto
{
    [System.ComponentModel.DataAnnotations.Required]
    public string ShippingFullName { get; set; } = string.Empty;
    
    [System.ComponentModel.DataAnnotations.Required]
    [System.ComponentModel.DataAnnotations.EmailAddress]
    public string ShippingEmail { get; set; } = string.Empty;
    
    [System.ComponentModel.DataAnnotations.Required]
    public string ShippingPhone { get; set; } = string.Empty;
    
    [System.ComponentModel.DataAnnotations.Required]
    public string ShippingAddress { get; set; } = string.Empty;
    
    [System.ComponentModel.DataAnnotations.Required]
    public string ShippingCity { get; set; } = string.Empty;
    
    [System.ComponentModel.DataAnnotations.Required]
    public string ShippingState { get; set; } = string.Empty;
    
    [System.ComponentModel.DataAnnotations.Required]
    public string ShippingZipCode { get; set; } = string.Empty;
    
    [System.ComponentModel.DataAnnotations.Required]
    public string ShippingCountry { get; set; } = string.Empty;
}
