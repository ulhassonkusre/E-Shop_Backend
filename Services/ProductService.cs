using EcommerceBackend.Models;

namespace EcommerceBackend.Services;

public interface IProductService
{
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

    public ProductService()
    {
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
            new Product { Name = "Laptop Stand", Description = "Ergonomic aluminum laptop stand for better posture.", Price = 49.99m, ImageUrl = "https://images.unsplash.com/photo-1527864550417-7fd91fc51a46?w=400", Stock = 100, Category = "Accessories" },
            new Product { Name = "Mechanical Keyboard", Description = "RGB mechanical gaming keyboard with Cherry MX switches.", Price = 129.99m, ImageUrl = "https://images.unsplash.com/photo-1587829741301-dc798b91a603?w=400", Stock = 45, Category = "Electronics" },
            new Product { Name = "USB-C Hub", Description = "7-in-1 USB-C hub with HDMI, USB 3.0, and SD card reader.", Price = 39.99m, ImageUrl = "https://images.unsplash.com/photo-1625842268584-8f3c8f5634c7?w=400", Stock = 80, Category = "Accessories" },
            new Product { Name = "Wireless Mouse", Description = "Ergonomic wireless mouse with precision tracking.", Price = 59.99m, ImageUrl = "https://images.unsplash.com/photo-1527864550417-7fd91fc51a46?w=400", Stock = 60, Category = "Electronics" },
            new Product { Name = "Monitor Stand", Description = "Adjustable monitor stand with storage drawer.", Price = 79.99m, ImageUrl = "https://images.unsplash.com/photo-1527864550417-7fd91fc51a46?w=400", Stock = 35, Category = "Accessories" },
            new Product { Name = "Webcam HD", Description = "1080p HD webcam with built-in microphone.", Price = 89.99m, ImageUrl = "https://images.unsplash.com/photo-1587829741301-dc798b91a603?w=400", Stock = 40, Category = "Electronics" },
            new Product { Name = "Desk Lamp", Description = "LED desk lamp with adjustable brightness and color temperature.", Price = 44.99m, ImageUrl = "https://images.unsplash.com/photo-1534073828943-f801091a7d58?w=400", Stock = 55, Category = "Home" },
            new Product { Name = "Bluetooth Speaker", Description = "Portable Bluetooth speaker with 360-degree sound.", Price = 79.99m, ImageUrl = "https://images.unsplash.com/photo-1608043152269-423dbba4e7e1?w=400", Stock = 70, Category = "Electronics" },
            new Product { Name = "Phone Stand", Description = "Adjustable phone stand for desk.", Price = 19.99m, ImageUrl = "https://images.unsplash.com/photo-1586953208448-b95a79798f07?w=400", Stock = 120, Category = "Accessories" },
            new Product { Name = "Cable Organizer", Description = "Desktop cable management system.", Price = 24.99m, ImageUrl = "https://images.unsplash.com/photo-1558002038-1091a1661116?w=400", Stock = 90, Category = "Accessories" }
        };

        foreach (var product in sampleProducts)
        {
            product.Id = _nextId++;
            Products.Add(product);
        }
    }

    public List<Product> GetAll()
    {
        return Products.ToList();
    }

    public Product? GetById(int id)
    {
        return Products.FirstOrDefault(p => p.Id == id);
    }

    public Product Create(CreateProductDto dto)
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
        return product;
    }

    public Product? Update(int id, UpdateProductDto dto)
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

        return product;
    }

    public bool Delete(int id)
    {
        var product = Products.FirstOrDefault(p => p.Id == id);
        if (product == null)
            return false;

        Products.Remove(product);
        return true;
    }

    public List<Product> Search(string searchTerm)
    {
        if (string.IsNullOrWhiteSpace(searchTerm))
            return Products.ToList();

        var term = searchTerm.ToLower();
        return Products.Where(p => 
            p.Name.ToLower().Contains(term) || 
            p.Description.ToLower().Contains(term) ||
            p.Category.ToLower().Contains(term)
        ).ToList();
    }
}
