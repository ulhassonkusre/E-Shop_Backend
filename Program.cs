using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using EcommerceBackend.Data;
using EcommerceBackend.Data.Entities;
using EcommerceBackend.Services;
using EcommerceBackend.Utilities;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        // Ensure DateTime is serialized as UTC with 'Z' suffix
        options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
        options.JsonSerializerOptions.Converters.Add(new JsonStringDateTimeConverter());
    });
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

// Configure TiDB Database with optimized settings
builder.Services.AddDbContext<EcommerceDbContext>(options =>
    options.UseMySql(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        new MySqlServerVersion(new Version(8, 0, 0)),
        dbOptions =>
        {
            dbOptions.EnableRetryOnFailure(
                maxRetryCount: 3,
                maxRetryDelay: TimeSpan.FromSeconds(5),
                errorNumbersToAdd: null
            );
            dbOptions.CommandTimeout(15); // Reduced from default 30
            dbOptions.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery);
        }
    ));

// Configure Redis for fast data retrieval (lazy connection)
var redisConfiguration = builder.Configuration.GetSection("Redis");
var redisConnection = redisConfiguration["ConnectionString"] ?? "localhost:6379";

// Register Redis Connection Multiplexer as singleton (lazy connection)
builder.Services.AddSingleton<StackExchange.Redis.IConnectionMultiplexer>(sp =>
{
    var config = StackExchange.Redis.ConfigurationOptions.Parse(redisConnection, true);
    config.AbortOnConnectFail = false; // Don't fail on startup if Redis is down
    config.SyncTimeout = 3000; // Reduced timeout
    config.AsyncTimeout = 3000; // Reduced timeout
    config.ConnectTimeout = 3000; // Faster connection timeout
    config.DefaultDatabase = 0;
    config.ConnectRetry = 2; // Fewer retries on startup
    return StackExchange.Redis.ConnectionMultiplexer.Connect(config);
});
builder.Services.AddSingleton<IRedisCacheService, RedisCacheService>();

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

// Initialize database in background (non-blocking)
Console.WriteLine("=== Initializing Database (Background) ===");
_ = Task.Run(async () =>
{
    using var scope = app.Services.CreateScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<EcommerceDbContext>();
    try
    {
        // Apply migrations with timeout
        Console.WriteLine("⏳ Applying database migrations...");
        var migrateTask = dbContext.Database.MigrateAsync();
        if (await Task.WhenAny(migrateTask, Task.Delay(30000)) == migrateTask)
        {
            await migrateTask;
            Console.WriteLine("✓ Database migrated successfully!");
        }
        else
        {
            Console.WriteLine("⚠ Database migration timeout - continuing anyway");
        }

        // Ensure default user exists (quick check)
        Console.WriteLine("⏳ Checking default user...");
        var userCountTask = dbContext.Users.CountAsync();
        if (await Task.WhenAny(userCountTask, Task.Delay(5000)) == userCountTask)
        {
            var userCount = await userCountTask;
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
        }

        // Skip product image updates on first run (not critical)
        Console.WriteLine("✓ Database initialization complete!");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"⚠ Database initialization warning: {ex.Message}");
        // Don't crash - app can still run
    }
}).ContinueWith(t => 
{
    if (t.IsFaulted)
    {
        Console.WriteLine($"⚠ Background initialization error: {t.Exception?.GetBaseException().Message}");
    }
});

Console.WriteLine("🚀 Application starting...");
app.Run("http://localhost:5000");
