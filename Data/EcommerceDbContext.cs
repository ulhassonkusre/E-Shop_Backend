using EcommerceBackend.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace EcommerceBackend.Data;

public class EcommerceDbContext : DbContext
{
    public EcommerceDbContext(DbContextOptions<EcommerceDbContext> options)
        : base(options)
    {
    }

    public DbSet<Product> Products { get; set; }
    public DbSet<User> Users { get; set; }
    public DbSet<CartItem> CartItems { get; set; }
    public DbSet<Order> Orders { get; set; }
    public DbSet<OrderItem> OrderItems { get; set; }
    public DbSet<WishlistItem> WishlistItems { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure Product with indexes for faster queries
        modelBuilder.Entity<Product>(entity =>
        {
            entity.HasIndex(e => e.Category);
            entity.HasIndex(e => e.Name);
            entity.HasIndex(e => e.Price);
        });

        // Configure User
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasIndex(e => e.Email).IsUnique();
            entity.HasIndex(e => e.Username).IsUnique();
        });

        // Configure CartItem
        modelBuilder.Entity<CartItem>(entity =>
        {
            entity.HasIndex(e => new { e.UserId, e.ProductId }).IsUnique();
            entity.HasOne(e => e.User)
                  .WithMany(u => u.CartItems)
                  .HasForeignKey(e => e.UserId)
                  .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.Product)
                  .WithMany(p => p.CartItems)
                  .HasForeignKey(e => e.ProductId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // Configure Order
        modelBuilder.Entity<Order>(entity =>
        {
            entity.HasIndex(e => e.OrderNumber).IsUnique();
            entity.HasIndex(e => e.UserId);
            entity.HasIndex(e => e.Status);
            entity.HasOne(e => e.User)
                  .WithMany(u => u.Orders)
                  .HasForeignKey(e => e.UserId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // Configure OrderItem
        modelBuilder.Entity<OrderItem>(entity =>
        {
            entity.HasIndex(e => e.OrderId);
            entity.HasIndex(e => e.ProductId);
            entity.HasOne(e => e.Order)
                  .WithMany(o => o.OrderItems)
                  .HasForeignKey(e => e.OrderId)
                  .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.Product)
                  .WithMany(p => p.OrderItems)
                  .HasForeignKey(e => e.ProductId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // Configure WishlistItem
        modelBuilder.Entity<WishlistItem>(entity =>
        {
            entity.HasIndex(e => new { e.UserId, e.ProductId }).IsUnique();
            entity.HasOne(e => e.User)
                  .WithMany(u => u.WishlistItems)
                  .HasForeignKey(e => e.UserId)
                  .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.Product)
                  .WithMany(p => p.WishlistItems)
                  .HasForeignKey(e => e.ProductId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // Seed initial products
        SeedProducts(modelBuilder);
        
        // Seed default user
        SeedDefaultUser(modelBuilder);
    }

    private static void SeedProducts(ModelBuilder modelBuilder)
    {
        var products = new[]
        {
            new Product { Id = 1, Name = "Wireless Headphones", Description = "Premium noise-canceling wireless headphones with 30-hour battery life.", Price = 199.99m, ImageUrl = "https://picsum.photos/seed/headphones/400/300", Stock = 50, Category = "Electronics" },
            new Product { Id = 2, Name = "Smart Watch", Description = "Feature-rich smartwatch with health tracking and GPS.", Price = 299.99m, ImageUrl = "https://picsum.photos/seed/smartwatch/400/300", Stock = 30, Category = "Electronics" },
            new Product { Id = 3, Name = "Laptop Stand", Description = "Ergonomic aluminum laptop stand for better posture.", Price = 49.99m, ImageUrl = "https://picsum.photos/seed/laptop/400/300", Stock = 100, Category = "Accessories" },
            new Product { Id = 4, Name = "Mechanical Keyboard", Description = "RGB mechanical gaming keyboard with Cherry MX switches.", Price = 129.99m, ImageUrl = "https://picsum.photos/seed/keyboard/400/300", Stock = 45, Category = "Electronics" },
            new Product { Id = 5, Name = "USB-C Hub", Description = "7-in-1 USB-C hub with HDMI, USB 3.0, and SD card reader.", Price = 39.99m, ImageUrl = "https://picsum.photos/seed/usb/400/300", Stock = 80, Category = "Accessories" },
            new Product { Id = 6, Name = "Wireless Mouse", Description = "Ergonomic wireless mouse with precision tracking.", Price = 59.99m, ImageUrl = "https://picsum.photos/seed/mouse/400/300", Stock = 60, Category = "Electronics" },
            new Product { Id = 7, Name = "Monitor Stand", Description = "Adjustable monitor stand with storage drawer.", Price = 79.99m, ImageUrl = "https://picsum.photos/seed/screen/400/300", Stock = 35, Category = "Accessories" },
            new Product { Id = 8, Name = "Webcam HD", Description = "1080p HD webcam with built-in microphone.", Price = 89.99m, ImageUrl = "https://picsum.photos/seed/camera/400/300", Stock = 40, Category = "Electronics" },
            new Product { Id = 9, Name = "Desk Lamp", Description = "LED desk lamp with adjustable brightness and color temperature.", Price = 44.99m, ImageUrl = "https://picsum.photos/seed/light/400/300", Stock = 55, Category = "Home" },
            new Product { Id = 10, Name = "Bluetooth Speaker", Description = "Portable Bluetooth speaker with 360-degree sound.", Price = 79.99m, ImageUrl = "https://picsum.photos/seed/audio/400/300", Stock = 70, Category = "Electronics" },
            new Product { Id = 11, Name = "Phone Stand", Description = "Adjustable phone stand for desk.", Price = 19.99m, ImageUrl = "https://picsum.photos/seed/mobile/400/300", Stock = 120, Category = "Accessories" },
            new Product { Id = 12, Name = "Cable Organizer", Description = "Desktop cable management system.", Price = 24.99m, ImageUrl = "https://picsum.photos/seed/tech/400/300", Stock = 90, Category = "Accessories" }
        };

        modelBuilder.Entity<Product>().HasData(products);
    }
    
    private static void SeedDefaultUser(ModelBuilder modelBuilder)
    {
        var user = new User 
        { 
            Id = 1,
            Username = "admin",
            Email = "admin@example.com",
            Password = BCrypt.Net.BCrypt.HashPassword("admin123"),
            CreatedAt = DateTime.UtcNow
        };
        
        modelBuilder.Entity<User>().HasData(user);
    }
}
