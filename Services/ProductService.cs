using EcommerceBackend.Models;

namespace EcommerceBackend.Services;

public interface IProductService
{
    Task<List<Product>> GetAllAsync();
    Task<Product?> GetByIdAsync(int id);
    Task<Product> CreateAsync(CreateProductDto dto);
    Task<Product?> UpdateAsync(int id, UpdateProductDto dto);
    Task<bool> DeleteAsync(int id);
    Task<List<Product>> SearchAsync(string searchTerm);
    List<Product> GetAll();
    Product? GetById(int id);
    Product Create(CreateProductDto dto);
    Product? Update(int id, UpdateProductDto dto);
    bool Delete(int id);
    List<Product> Search(string searchTerm);
}

public class ProductService : IProductService
{
    // In-memory storage
    private static readonly List<Product> Products = new();
    private static int _nextId = 1;
    private readonly IRedisCacheService _cache;
    private readonly TimeSpan _cacheExpiry = TimeSpan.FromMinutes(30);

    // Cache keys
    private const string PRODUCTS_ALL_KEY = "products:all";
    private const string PRODUCTS_BY_ID_KEY = "products:id:{0}";
    private const string PRODUCTS_SEARCH_KEY = "products:search:{0}";

    public ProductService(IRedisCacheService cache)
    {
        _cache = cache;
        // Seed sample products
        if (!Products.Any())
        {
            SeedProducts();
        }
    }

    private void SeedProducts()
    {
        var sampleProducts = new[]
        {
            new Product { Name = "Wireless Headphones", Description = "Premium noise-canceling wireless headphones with 30-hour battery life.", Price = 199.99m, ImageUrl = "https://images.unsplash.com/photo-1505740420928-5e560c06d30e?w=400", Stock = 50, Category = "Electronics" },
            new Product { Name = "Smart Watch", Description = "Feature-rich smartwatch with health tracking and GPS.", Price = 299.99m, ImageUrl = "https://images.unsplash.com/photo-1523275335684-37898b6baf30?w=400", Stock = 30, Category = "Electronics" },
            new Product { Name = "Laptop Stand", Description = "Ergonomic aluminum laptop stand for better posture.", Price = 49.99m, ImageUrl = "https://images.unsplash.com/photo-1610557892470-55d9e80c0bce?w=400", Stock = 100, Category = "Accessories" },
            new Product { Name = "Mechanical Keyboard", Description = "RGB mechanical gaming keyboard with Cherry MX switches.", Price = 129.99m, ImageUrl = "https://images.unsplash.com/photo-1511467687858-23d96c32e4ae?w=400", Stock = 45, Category = "Electronics" },
            new Product { Name = "USB-C Hub", Description = "7-in-1 USB-C hub with HDMI, USB 3.0, and SD card reader.", Price = 39.99m, ImageUrl = "https://images.unsplash.com/photo-1610557892470-55d9e80c0bce?w=400", Stock = 80, Category = "Accessories" },
            new Product { Name = "Wireless Mouse", Description = "Ergonomic wireless mouse with precision tracking.", Price = 59.99m, ImageUrl = "https://images.unsplash.com/photo-1610557892470-55d9e80c0bce?w=400", Stock = 60, Category = "Electronics" },
            new Product { Name = "Monitor Stand", Description = "Adjustable monitor stand with storage drawer.", Price = 79.99m, ImageUrl = "https://images.unsplash.com/photo-1610557892470-55d9e80c0bce?w=400", Stock = 35, Category = "Accessories" },
            new Product { Name = "Webcam HD", Description = "1080p HD webcam with built-in microphone.", Price = 89.99m, ImageUrl = "https://images.unsplash.com/photo-1587829741301-dc798b91a603?w=400", Stock = 40, Category = "Electronics" },
            new Product { Name = "Desk Lamp", Description = "LED desk lamp with adjustable brightness and color temperature.", Price = 44.99m, ImageUrl = "https://images.unsplash.com/photo-1507440658841-9a2dd3a70d17?w=400", Stock = 55, Category = "Home" },
            new Product { Name = "Bluetooth Speaker", Description = "Portable Bluetooth speaker with 360-degree sound.", Price = 79.99m, ImageUrl = "https://images.unsplash.com/photo-1608043152269-423dbba4e7e1?w=400", Stock = 70, Category = "Electronics" },
            new Product { Name = "Phone Stand", Description = "Adjustable phone stand for desk.", Price = 19.99m, ImageUrl = "https://images.unsplash.com/photo-1586953208448-b95a79798f07?w=400", Stock = 120, Category = "Accessories" },
            new Product { Name = "Cable Organizer", Description = "Desktop cable management system.", Price = 24.99m, ImageUrl = "https://images.unsplash.com/photo-1523275335684-37898b6baf30?w=400", Stock = 90, Category = "Accessories" }
        };

        foreach (var product in sampleProducts)
        {
            product.Id = _nextId++;
            Products.Add(product);
        }
    }

    // Synchronous methods for backward compatibility
    public List<Product> GetAll()
    {
        return GetAllAsync().Result;
    }

    public Product? GetById(int id)
    {
        return GetByIdAsync(id).Result;
    }

    public Product Create(CreateProductDto dto)
    {
        return CreateAsync(dto).Result;
    }

    public Product? Update(int id, UpdateProductDto dto)
    {
        return UpdateAsync(id, dto).Result;
    }

    public bool Delete(int id)
    {
        return DeleteAsync(id).Result;
    }

    public List<Product> Search(string searchTerm)
    {
        return SearchAsync(searchTerm).Result;
    }

    // Async methods with Redis caching
    public async Task<List<Product>> GetAllAsync()
    {
        // Try to get from cache first
        var cachedProducts = await _cache.GetAsync<List<Product>>(PRODUCTS_ALL_KEY);
        if (cachedProducts != null)
        {
            return cachedProducts;
        }

        // If not in cache, get from memory and cache it
        var products = Products.ToList();
        await _cache.SetAsync(PRODUCTS_ALL_KEY, products, _cacheExpiry);
        return products;
    }

    public async Task<Product?> GetByIdAsync(int id)
    {
        var cacheKey = string.Format(PRODUCTS_BY_ID_KEY, id);
        
        // Try to get from cache first
        var cachedProduct = await _cache.GetAsync<Product>(cacheKey);
        if (cachedProduct != null)
        {
            return cachedProduct;
        }

        // If not in cache, get from memory and cache it
        var product = Products.FirstOrDefault(p => p.Id == id);
        if (product != null)
        {
            await _cache.SetAsync(cacheKey, product, _cacheExpiry);
        }
        return product;
    }

    public async Task<Product> CreateAsync(CreateProductDto dto)
    {
        var product = new Product
        {
            Id = _nextId++,
            Name = dto.Name,
            Description = dto.Description,
            Price = dto.Price,
            ImageUrl = dto.ImageUrl,
            Stock = dto.Stock,
            Category = dto.Category
        };

        Products.Add(product);
        
        // Invalidate cache
        await _cache.RemoveAsync(PRODUCTS_ALL_KEY);
        
        return product;
    }

    public async Task<Product?> UpdateAsync(int id, UpdateProductDto dto)
    {
        var product = Products.FirstOrDefault(p => p.Id == id);
        if (product == null)
            return null;

        if (dto.Name != null) product.Name = dto.Name;
        if (dto.Description != null) product.Description = dto.Description;
        if (dto.Price.HasValue) product.Price = dto.Price.Value;
        if (dto.ImageUrl != null) product.ImageUrl = dto.ImageUrl;
        if (dto.Stock.HasValue) product.Stock = dto.Stock.Value;
        if (dto.Category != null) product.Category = dto.Category;

        // Invalidate cache
        await _cache.RemoveAsync(PRODUCTS_ALL_KEY);
        await _cache.RemoveAsync(string.Format(PRODUCTS_BY_ID_KEY, id));

        return product;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var product = Products.FirstOrDefault(p => p.Id == id);
        if (product == null)
            return false;

        Products.Remove(product);
        
        // Invalidate cache
        await _cache.RemoveAsync(PRODUCTS_ALL_KEY);
        await _cache.RemoveAsync(string.Format(PRODUCTS_BY_ID_KEY, id));
        
        return true;
    }

    public async Task<List<Product>> SearchAsync(string searchTerm)
    {
        if (string.IsNullOrWhiteSpace(searchTerm))
        {
            return await GetAllAsync();
        }

        var cacheKey = string.Format(PRODUCTS_SEARCH_KEY, searchTerm.ToLower());
        
        // Try to get from cache first
        var cachedProducts = await _cache.GetAsync<List<Product>>(cacheKey);
        if (cachedProducts != null)
        {
            return cachedProducts;
        }

        // If not in cache, search and cache it
        var term = searchTerm.ToLower();
        var products = Products.Where(p =>
            p.Name.ToLower().Contains(term) ||
            p.Description.ToLower().Contains(term) ||
            p.Category.ToLower().Contains(term)
        ).ToList();

        await _cache.SetAsync(cacheKey, products, TimeSpan.FromMinutes(10));
        return products;
    }
}
