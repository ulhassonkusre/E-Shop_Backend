using EcommerceBackend.Models;
using EcommerceBackend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace EcommerceBackend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("signup")]
    public ActionResult<AuthResponseDto> SignUp([FromBody] RegisterDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var result = _authService.Register(dto);
        if (result == null)
            return Conflict(new { message = "Email already registered" });

        return Ok(result);
    }

    [HttpPost("login")]
    public ActionResult<AuthResponseDto> Login([FromBody] LoginDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var result = _authService.Login(dto);
        if (result == null)
            return Unauthorized(new { message = "Invalid email or password" });

        return Ok(result);
    }

    [Authorize]
    [HttpGet("me")]
    public ActionResult<AuthResponseDto> GetCurrentUser()
    {
        var userIdClaim = User.FindFirst("userId")?.Value;
        var nameClaim = User.FindFirst(ClaimTypes.Name)?.Value;
        var emailClaim = User.FindFirst(ClaimTypes.Email)?.Value;

        if (string.IsNullOrEmpty(userIdClaim))
            return Unauthorized();

        if (int.TryParse(userIdClaim, out int userId))
        {
            return Ok(new AuthResponseDto
            {
                UserId = userId,
                Username = nameClaim ?? "",
                Email = emailClaim ?? "",
                Token = ""
            });
        }

        return Unauthorized();
    }
}
