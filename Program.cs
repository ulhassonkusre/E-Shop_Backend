using System.Text;
using EcommerceBackend.Data;
using EcommerceBackend.Data.Entities;
using EcommerceBackend.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Ecommerce API", Version = "v1" });
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Enter 'Bearer' [space] and then your token.",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// Configure JWT Authentication
var jwtKey = builder.Configuration["Jwt:Key"] ?? "DefaultSecretKeyForJwtTokenGeneration123456";
var key = Encoding.UTF8.GetBytes(jwtKey);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ValidateIssuer = false,
        ValidateAudience = false,
        ClockSkew = TimeSpan.Zero
    };
});

builder.Services.AddAuthorization();

// Configure TiDB Database
builder.Services.AddDbContext<EcommerceDbContext>(options =>
    options.UseMySql(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        new MySqlServerVersion(new Version(8, 0, 0)),
        dbOptions => dbOptions.EnableRetryOnFailure(
            maxRetryCount: 3,
            maxRetryDelay: TimeSpan.FromSeconds(30),
            errorNumbersToAdd: null
        )
    ));

// Configure Redis (optional caching - disabled for performance)
var redisConfiguration = builder.Configuration.GetSection("Redis");
var redisConnection = redisConfiguration["ConnectionString"] ?? "localhost:6379";

// Redis disabled for performance - using hardcoded products directly
// builder.Services.AddSingleton<StackExchange.Redis.IConnectionMultiplexer>(sp =>
// {
//     var config = StackExchange.Redis.ConfigurationOptions.Parse(redisConnection, true);
//     config.AbortOnConnectFail = false;
//     config.SyncTimeout = 5000;
//     config.AsyncTimeout = 5000;
//     return StackExchange.Redis.ConnectionMultiplexer.Connect(config);
// });
// builder.Services.AddSingleton<IRedisCacheService, RedisCacheService>();

// Register services (use Scoped for database services)
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<ICartService, CartService>();
builder.Services.AddScoped<IOrderService, OrderService>();

// Configure CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngular", policy =>
    {
        policy.WithOrigins("http://localhost:4200")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowAngular");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Initialize database and seed data
Console.WriteLine("=== Initializing Database ===");
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<EcommerceDbContext>();
    try
    {
        await dbContext.Database.MigrateAsync();
        Console.WriteLine("✓ Database migrated successfully!");
        
        // Ensure default user exists
        var userCount = await dbContext.Users.CountAsync();
        if (userCount == 0)
        {
            dbContext.Users.Add(new User
            {
                Username = "admin",
                Email = "admin@example.com",
                Password = BCrypt.Net.BCrypt.HashPassword("admin123"),
                CreatedAt = DateTime.UtcNow
            });
            await dbContext.SaveChangesAsync();
            Console.WriteLine("✓ Default user created: admin@example.com / admin123");
        }
        else
        {
            Console.WriteLine($"✓ Users found: {userCount}");
        }
        
        // Update product images - fix empty or old Unsplash URLs
        var products = await dbContext.Products.ToListAsync();
        bool imagesUpdated = false;
        var imageMap = new Dictionary<int, string>
        {
            { 1, "https://picsum.photos/seed/headphones/400/300" },
            { 2, "https://picsum.photos/seed/smartwatch/400/300" },
            { 3, "https://picsum.photos/seed/laptop/400/300" },
            { 4, "https://picsum.photos/seed/keyboard/400/300" },
            { 5, "https://picsum.photos/seed/usb/400/300" },
            { 6, "https://picsum.photos/seed/mouse/400/300" },
            { 7, "https://picsum.photos/seed/screen/400/300" },
            { 8, "https://picsum.photos/seed/camera/400/300" },
            { 9, "https://picsum.photos/seed/light/400/300" },
            { 10, "https://picsum.photos/seed/audio/400/300" },
            { 11, "https://picsum.photos/seed/mobile/400/300" },
            { 12, "https://picsum.photos/seed/tech/400/300" }
        };

        foreach (var product in products)
        {
            if (imageMap.ContainsKey(product.Id) && (string.IsNullOrEmpty(product.ImageUrl) || product.ImageUrl.Contains("unsplash")))
            {
                product.ImageUrl = imageMap[product.Id];
                imagesUpdated = true;
            }
        }
        
        if (imagesUpdated)
        {
            await dbContext.SaveChangesAsync();
            Console.WriteLine("✓ Product images updated to Picsum Photos");
        }
        
        var productCount = await dbContext.Products.CountAsync();
        Console.WriteLine($"✓ Products in database: {productCount}");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"✗ Error initializing database: {ex.Message}");
        Console.WriteLine($"Stack trace: {ex.StackTrace}");
    }
}

Console.WriteLine("\n=== Starting Server ===");
app.Run("http://localhost:5000");
