using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using EcommerceBackend.Models;
using Microsoft.IdentityModel.Tokens;

namespace EcommerceBackend.Services;

public interface IAuthService
{
    AuthResponseDto? Register(RegisterDto dto);
    AuthResponseDto? Login(LoginDto dto);
    int? GetUserIdFromToken(string token);
}

public class AuthService : IAuthService
{
    private readonly IConfiguration _configuration;
    
    // In-memory storage
    private static readonly List<User> Users = new();
    private static int _nextId = 1;

    public AuthService(IConfiguration configuration)
    {
        _configuration = configuration;
        
        // Seed a default user for testing
        if (!Users.Any())
        {
            Users.Add(new User
            {
                Id = _nextId++,
                Username = "admin",
                Email = "admin@example.com",
                Password = BCrypt.Net.BCrypt.HashPassword("admin123")
            });
        }
    }

    public AuthResponseDto? Register(RegisterDto dto)
    {
        if (Users.Any(u => u.Email == dto.Email))
            return null;

        var user = new User
        {
            Id = _nextId++,
            Username = dto.Username,
            Email = dto.Email,
            Password = BCrypt.Net.BCrypt.HashPassword(dto.Password)
        };

        Users.Add(user);
        return CreateAuthResponse(user);
    }

    public AuthResponseDto? Login(LoginDto dto)
    {
        var user = Users.FirstOrDefault(u => u.Email == dto.Email);
        if (user == null || !BCrypt.Net.BCrypt.Verify(dto.Password, user.Password))
            return null;

        return CreateAuthResponse(user);
    }

    public int? GetUserIdFromToken(string token)
    {
        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_configuration["Jwt:Key"] ?? "DefaultSecretKeyForDevelopment123456");
            
            tokenHandler.ValidateToken(token, new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = false,
                ValidateAudience = false,
                ClockSkew = TimeSpan.Zero
            }, out SecurityToken validatedToken);

            var jwtToken = (JwtSecurityToken)validatedToken;
            var userIdClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == "userId");
            
            if (userIdClaim != null && int.TryParse(userIdClaim.Value, out int userId))
                return userId;
        }
        catch
        {
            return null;
        }
        
        return null;
    }

    private AuthResponseDto CreateAuthResponse(User user)
    {
        var token = GenerateJwtToken(user);
        
        return new AuthResponseDto
        {
            UserId = user.Id,
            Username = user.Username,
            Email = user.Email,
            Token = token
        };
    }

    private string GenerateJwtToken(User user)
    {
        var key = Encoding.UTF8.GetBytes(_configuration["Jwt:Key"] ?? "DefaultSecretKeyForDevelopment123456");
        var issuer = _configuration["Jwt:Issuer"] ?? "EcommerceBackend";
        var audience = _configuration["Jwt:Audience"] ?? "EcommerceFrontend";

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim("userId", user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.Username),
            new Claim(ClaimTypes.Email, user.Email)
        };

        var credentials = new SigningCredentials(
            new SymmetricSecurityKey(key),
            SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: DateTime.UtcNow.AddDays(7),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
