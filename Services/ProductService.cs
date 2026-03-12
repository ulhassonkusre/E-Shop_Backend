using EcommerceBackend.Data;
using EcommerceBackend.Data.Entities;
using EcommerceBackend.Models;
using Microsoft.EntityFrameworkCore;

namespace EcommerceBackend.Services;

public interface IProductService
{
    Task<List<Models.Product>> GetAllAsync();
    Task<Models.Product?> GetByIdAsync(int id);
    Task<Models.Product> CreateAsync(CreateProductDto dto);
    Task<Models.Product?> UpdateAsync(int id, UpdateProductDto dto);
    Task<bool> DeleteAsync(int id);
    Task<List<Models.Product>> SearchAsync(string searchTerm);
    List<Models.Product> GetAll();
    Models.Product? GetById(int id);
    Models.Product Create(CreateProductDto dto);
    Models.Product? Update(int id, UpdateProductDto dto);
    bool Delete(int id);
    List<Models.Product> Search(string searchTerm);
}

public class ProductService : IProductService
{
    private readonly EcommerceDbContext _context;
    private readonly IRedisCacheService? _cache;
    private readonly TimeSpan _cacheExpiry = TimeSpan.FromMinutes(30);
    private readonly ILogger<ProductService>? _logger;

    private const string PRODUCTS_ALL_KEY = "products:all";
    private const string PRODUCTS_BY_ID_KEY = "products:id:{0}";
    private const string PRODUCTS_SEARCH_KEY = "products:search:{0}";

    // Hardcoded product list
    private static readonly List<Models.Product> HardcodedProducts = new()
    {
        new Models.Product { Id = 1, Name = "Wireless Headphones", Description = "Premium noise-canceling wireless headphones with 30-hour battery life.", Price = 199.99m, ImageUrl = "https://images.unsplash.com/photo-1505740420928-5e560c06d30e?w=400", Stock = 50, Category = "Electronics", CreatedAt = DateTime.UtcNow },
        new Models.Product { Id = 2, Name = "Smart Watch", Description = "Feature-rich smartwatch with health tracking and GPS.", Price = 299.99m, ImageUrl = "https://images.unsplash.com/photo-1523275335684-37898b6baf30?w=400", Stock = 30, Category = "Electronics", CreatedAt = DateTime.UtcNow },
        new Models.Product { Id = 3, Name = "Laptop Stand", Description = "Ergonomic aluminum laptop stand for better posture.", Price = 49.99m, ImageUrl = "https://images.unsplash.com/photo-1527864550417-7fd91fc51a46?w=400", Stock = 100, Category = "Accessories", CreatedAt = DateTime.UtcNow },
        //new Models.Product { Id = 4, Name = "Mechanical Keyboard", Description = "RGB mechanical gaming keyboard with Cherry MX switches.", Price = 129.99m, ImageUrl = "https://images.unsplash.com/photo-1587829741301-dc798b91a603?w=400", Stock = 45, Category = "Electronics", CreatedAt = DateTime.UtcNow },
       // new Models.Product { Id = 5, Name = "USB-C Hub", Description = "7-in-1 USB-C hub with HDMI, USB 3.0, and SD card reader.", Price = 39.99m, ImageUrl = "https://images.unsplash.com/photo-1625842268584-8f3c8f5634c7?w=400", Stock = 80, Category = "Accessories", CreatedAt = DateTime.UtcNow },
        new Models.Product { Id = 6, Name = "Mechanical Keyboard", Description = "RGB mechanical gaming keyboard with Cherry MX switches.", Price = 129.99m, ImageUrl = "https://images.unsplash.com/photo-1511467687858-23d96c32e4ae", Stock = 45, Category = "Electronics", CreatedAt = DateTime.UtcNow },
        new Models.Product { Id = 7, Name = "USB-C Hub", Description = "7-in-1 USB-C hub with HDMI, USB 3.0, and SD card reader.", Price = 39.99m, ImageUrl = "https://images.unsplash.com/photo-1610557892470-55d9e80c0bce?w=400", Stock = 80, Category = "Accessories", CreatedAt = DateTime.UtcNow },
        new Models.Product { Id = 8, Name = "Wireless Mouse", Description = "Ergonomic wireless mouse with precision tracking.", Price = 59.99m, ImageUrl = "https://images.unsplash.com/photo-1527864550417-7fd91fc51a46?w=400", Stock = 60, Category = "Electronics", CreatedAt = DateTime.UtcNow },
        new Models.Product { Id = 9, Name = "Monitor Stand", Description = "Adjustable monitor stand with storage drawer.", Price = 79.99m, ImageUrl = "https://images.unsplash.com/photo-1527864550417-7fd91fc51a46?w=400", Stock = 35, Category = "Accessories", CreatedAt = DateTime.UtcNow },
        new Models.Product { Id = 10, Name = "Webcam HD", Description = "1080p HD webcam with built-in microphone.", Price = 89.99m, ImageUrl = "https://images.unsplash.com/photo-1587829741301-dc798b91a603?w=400", Stock = 40, Category = "Electronics", CreatedAt = DateTime.UtcNow },
        //new Models.Product { Id = 11, Name = "Desk Lamp", Description = "LED desk lamp with adjustable brightness and color temperature.", Price = 44.99m, ImageUrl = "https://images.unsplash.com/photo-1534073828943-f801091a7d58?w=400", Stock = 55, Category = "Home", CreatedAt = DateTime.UtcNow },
        new Models.Product { Id = 12, Name = "Webcam HD", Description = "1080p HD webcam with built-in microphone.", Price = 89.99m, ImageUrl = "https://images.unsplash.com/photo-1511467687858-23d96c32e4ae", Stock = 40, Category = "Electronics", CreatedAt = DateTime.UtcNow },
        new Models.Product { Id = 13, Name = "Desk Lamp", Description = "LED desk lamp with adjustable brightness and color temperature.", Price = 44.99m, ImageUrl = "https://images.unsplash.com/photo-1523275335684-37898b6baf30?w=400", Stock = 55, Category = "Home", CreatedAt = DateTime.UtcNow },
        new Models.Product { Id = 14, Name = "Bluetooth Speaker", Description = "Portable Bluetooth speaker with 360-degree sound.", Price = 79.99m, ImageUrl = "https://images.unsplash.com/photo-1608043152269-423dbba4e7e1?w=400", Stock = 70, Category = "Electronics", CreatedAt = DateTime.UtcNow },
        new Models.Product { Id = 15, Name = "Phone Stand", Description = "Adjustable phone stand for desk.", Price = 19.99m, ImageUrl = "https://images.unsplash.com/photo-1586953208448-b95a79798f07?w=400", Stock = 120, Category = "Accessories", CreatedAt = DateTime.UtcNow },
        new Models.Product { Id = 16, Name = "Cable Organizer", Description = "Desktop cable management system.", Price = 24.99m, ImageUrl = "https://images.unsplash.com/photo-1558002038-1091a1661116?w=400", Stock = 90, Category = "Accessories", CreatedAt = DateTime.UtcNow },
        new Models.Product { Id = 17, Name = "Cable Organizer", Description = "Desktop cable management system.", Price = 24.99m, ImageUrl = "https://images.unsplash.com/photo-1610557892470-55d9e80c0bce", Stock = 90, Category = "Accessories", CreatedAt = DateTime.UtcNow }
    };

    public ProductService(EcommerceDbContext context, IRedisCacheService? cache = null, ILogger<ProductService>? logger = null)
    {
        _context = context;
        _cache = cache;
        _logger = logger;
    }

    // Synchronous methods for backward compatibility
    public List<Models.Product> GetAll() => GetAllAsync().Result;
    public Models.Product? GetById(int id) => GetByIdAsync(id).Result;
    public Models.Product Create(CreateProductDto dto) => CreateAsync(dto).Result;
    public Models.Product? Update(int id, UpdateProductDto dto) => UpdateAsync(id, dto).Result;
    public bool Delete(int id) => DeleteAsync(id).Result;
    public List<Models.Product> Search(string searchTerm) => SearchAsync(searchTerm).Result;

    // Async methods - using hardcoded products directly (Redis disabled for performance)
    public async Task<List<Models.Product>> GetAllAsync()
    {
        // Return hardcoded products directly - instant response
        await Task.Yield(); // Ensure async context
        return HardcodedProducts;
    }

    public async Task<Models.Product?> GetByIdAsync(int id)
    {
        // Return hardcoded product directly - instant response
        await Task.Yield();
        return HardcodedProducts.FirstOrDefault(p => p.Id == id);
    }

    public async Task<Models.Product> CreateAsync(CreateProductDto dto)
    {
        var product = new Data.Entities.Product
        {
            Name = dto.Name,
            Description = dto.Description,
            Price = dto.Price,
            ImageUrl = dto.ImageUrl,
            Stock = dto.Stock,
            Category = dto.Category,
            CreatedAt = DateTime.UtcNow
        };

        _context.Products.Add(product);
        await _context.SaveChangesAsync();

        // Cache removed for performance - hardcoded products used instead
        _logger?.LogInformation("Product created: {ProductId} - {ProductName}", product.Id, product.Name);
        return MapToDto(product);
    }

    public async Task<Models.Product?> UpdateAsync(int id, UpdateProductDto dto)
    {
        var product = await _context.Products.FindAsync(id);
        if (product == null)
            return null;

        if (dto.Name != null) product.Name = dto.Name;
        if (dto.Description != null) product.Description = dto.Description;
        if (dto.Price.HasValue) product.Price = dto.Price.Value;
        if (dto.ImageUrl != null) product.ImageUrl = dto.ImageUrl;
        if (dto.Stock.HasValue) product.Stock = dto.Stock.Value;
        if (dto.Category != null) product.Category = dto.Category;
        product.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        // Cache removed for performance - hardcoded products used instead
        _logger?.LogInformation("Product updated: {ProductId} - {ProductName}", product.Id, product.Name);
        return MapToDto(product);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var product = await _context.Products.FindAsync(id);
        if (product == null)
            return false;

        _context.Products.Remove(product);
        await _context.SaveChangesAsync();

        // Cache removed for performance - hardcoded products used instead
        _logger?.LogInformation("Product deleted: {ProductId}", product.Id);
        return true;
    }

    public async Task<List<Models.Product>> SearchAsync(string searchTerm)
    {
        if (string.IsNullOrWhiteSpace(searchTerm))
        {
            return await GetAllAsync();
        }

        // Search hardcoded products directly - instant response
        await Task.Yield();
        var term = searchTerm.ToLower();
        return HardcodedProducts
            .Where(p => p.Name.ToLower().Contains(term) ||
                       p.Description.ToLower().Contains(term) ||
                       p.Category.ToLower().Contains(term))
            .ToList();
    }

    private static Models.Product MapToDto(Data.Entities.Product product)
    {
        return new Models.Product
        {
            Id = product.Id,
            Name = product.Name,
            Description = product.Description,
            Price = product.Price,
            ImageUrl = product.ImageUrl,
            Stock = product.Stock,
            Category = product.Category,
            CreatedAt = product.CreatedAt,
            UpdatedAt = product.UpdatedAt
        };
    }
}
