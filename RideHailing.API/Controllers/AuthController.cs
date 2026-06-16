// ============================================================
// Controllers/AuthController.cs — Registration & Logins
// Handles user access, JWT issuance, and session refresh tokens
// ============================================================
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RideHailing.API.Models;
using RideHailing.API.Services;

namespace RideHailing.API.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController(IAuthService authService) : ControllerBase
{
    // POST: api/auth/register
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest req)
    {
        var result = await authService.RegisterAsync(req);
        if (result == null) 
            return Conflict(new { message = "Email address or phone number already in use." });
            
        return Ok(result);
    }

    // POST: api/auth/login
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest req)
    {
        var result = await authService.LoginAsync(req);
        if (result == null) 
            return Unauthorized(new { message = "Invalid email address or password credentials." });
            
        return Ok(result);
    }

    // POST: api/auth/refresh
    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh([FromBody] string refreshToken)
    {
        var result = await authService.RefreshTokenAsync(refreshToken);
        if (result == null) 
            return Unauthorized(new { message = "Invalid or expired session refresh token." });
            
        return Ok(result);
    }

    // POST: api/auth/logout
    [Authorize]
    [HttpPost("logout")]
    public async Task<IActionResult> Logout([FromBody] string refreshToken)
    {
        await authService.RevokeRefreshTokenAsync(refreshToken);
        return NoContent();
    }
}
