using System.ComponentModel.DataAnnotations;

namespace EcommerceBackend.Models;

public class Product
{
    public int Id { get; set; }
    
    [Required]
    public string Name { get; set; } = string.Empty;
    
    [Required]
    public string Description { get; set; } = string.Empty;
    
    [Required]
    public decimal Price { get; set; }
    
    [Required]
    public string ImageUrl { get; set; } = string.Empty;
    
    public int Stock { get; set; }
    
    [Required]
    public string Category { get; set; } = string.Empty;
    
    public DateTime CreatedAt { get; set; }
    
    public DateTime? UpdatedAt { get; set; }
}

public class CreateProductDto
{
    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;
    
    [Required]
    public string Description { get; set; } = string.Empty;
    
    [Required]
    public decimal Price { get; set; }
    
    [Required]
    public string ImageUrl { get; set; } = string.Empty;
    
    public int Stock { get; set; }
    
    [Required]
    [MaxLength(100)]
    public string Category { get; set; } = string.Empty;
}

public class UpdateProductDto
{
    [MaxLength(200)]
    public string? Name { get; set; }
    
    public string? Description { get; set; }
    
    public decimal? Price { get; set; }
    
    public string? ImageUrl { get; set; }
    
    public int? Stock { get; set; }
    
    [MaxLength(100)]
    public string? Category { get; set; }
}
