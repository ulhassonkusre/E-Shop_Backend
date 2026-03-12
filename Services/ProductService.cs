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
    private readonly IRedisCacheService _cache;
    private readonly TimeSpan _cacheExpiry = TimeSpan.FromMinutes(30);
    private readonly ILogger<ProductService> _logger;

    private const string PRODUCTS_ALL_KEY = "products:all";
    private const string PRODUCTS_BY_ID_KEY = "products:id:{0}";
    private const string PRODUCTS_SEARCH_KEY = "products:search:{0}";

    public ProductService(EcommerceDbContext context, IRedisCacheService cache, ILogger<ProductService> logger)
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

    /// <summary>
    /// Get all products from database with Redis caching
    /// </summary>
    public async Task<List<Models.Product>> GetAllAsync()
    {
        // Try to get from Redis cache first
        var cachedProducts = await _cache.GetAsync<List<Models.Product>>(PRODUCTS_ALL_KEY);
        if (cachedProducts != null && cachedProducts.Any())
        {
            _logger.LogDebug("Cache hit for all products");
            return cachedProducts;
        }

        // Cache miss - fetch from database
        _logger.LogDebug("Cache miss for all products - fetching from database");
        var dbProducts = await _context.Products.ToListAsync();
        var products = dbProducts.Select(MapToDto).ToList();

        // Cache the results
        if (products.Any())
        {
            await _cache.SetAsync(PRODUCTS_ALL_KEY, products, _cacheExpiry);
        }
        
        return products;
    }

    /// <summary>
    /// Get product by ID from database with Redis caching
    /// </summary>
    public async Task<Models.Product?> GetByIdAsync(int id)
    {
        var cacheKey = string.Format(PRODUCTS_BY_ID_KEY, id);

        // Try to get from Redis cache first
        var cachedProduct = await _cache.GetAsync<Models.Product>(cacheKey);
        if (cachedProduct != null)
        {
            _logger.LogDebug("Cache hit for product ID: {ProductId}", id);
            return cachedProduct;
        }

        // Cache miss - fetch from database
        _logger.LogDebug("Cache miss for product ID: {ProductId} - fetching from database", id);
        var dbProduct = await _context.Products.FindAsync(id);
        
        Models.Product? product = null;
        
        if (dbProduct != null)
        {
            product = MapToDto(dbProduct);
            
            // Cache the result
            await _cache.SetAsync(cacheKey, product, _cacheExpiry);
        }

        return product;
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

        // Invalidate cache to ensure fresh data
        await _cache.RemoveByPatternAsync("products:*");
        _logger.LogInformation("Product created: {ProductId} - {ProductName}, cache invalidated", product.Id, product.Name);
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

        // Invalidate cache to ensure fresh data
        await _cache.RemoveByPatternAsync("products:*");
        _logger.LogInformation("Product updated: {ProductId} - {ProductName}, cache invalidated", product.Id, product.Name);
        return MapToDto(product);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var product = await _context.Products.FindAsync(id);
        if (product == null)
            return false;

        _context.Products.Remove(product);
        await _context.SaveChangesAsync();

        // Invalidate cache to ensure fresh data
        await _cache.RemoveByPatternAsync("products:*");
        _logger.LogInformation("Product deleted: {ProductId}, cache invalidated", product.Id);
        return true;
    }

    /// <summary>
    /// Search products in database with Redis caching
    /// </summary>
    public async Task<List<Models.Product>> SearchAsync(string searchTerm)
    {
        if (string.IsNullOrWhiteSpace(searchTerm))
        {
            return await GetAllAsync();
        }

        var cacheKey = string.Format(PRODUCTS_SEARCH_KEY, searchTerm.ToLower().Replace(" ", "-"));
        
        // Try to get from Redis cache first
        var cachedProducts = await _cache.GetAsync<List<Models.Product>>(cacheKey);
        if (cachedProducts != null && cachedProducts.Any())
        {
            _logger.LogDebug("Cache hit for search term: {SearchTerm}", searchTerm);
            return cachedProducts;
        }

        // Cache miss - search database
        _logger.LogDebug("Cache miss for search term: {SearchTerm} - searching database", searchTerm);
        var term = searchTerm.ToLower();
        var dbProducts = await _context.Products
            .Where(p => p.Name.ToLower().Contains(term) ||
                       p.Description.ToLower().Contains(term) ||
                       p.Category.ToLower().Contains(term))
            .ToListAsync();

        var products = dbProducts.Select(MapToDto).ToList();

        // Cache the results
        if (products.Any())
        {
            await _cache.SetAsync(cacheKey, products, _cacheExpiry);
        }
        
        return products;
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
