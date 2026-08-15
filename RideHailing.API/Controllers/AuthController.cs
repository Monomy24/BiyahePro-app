using Microsoft.AspNetCore.Mvc;
using RideHailing.API.Models;
using RideHailing.API.Services;

namespace RideHailing.API.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController(IAuthService authService) : ControllerBase
{
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        var result = await authService.RegisterAsync(request);
        if (result == null) return BadRequest("Email already exists.");
        return Ok(result);
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var result = await authService.LoginAsync(request);
        if (result == null) return Unauthorized();
        return Ok(result);
    }

    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh([FromBody] string refreshToken)
    {
        var result = await authService.RefreshTokenAsync(refreshToken);
        if (result == null) return Unauthorized();
        return Ok(result);
    }

    [HttpPost("revoke")]
    public async Task<IActionResult> Revoke([FromBody] string refreshToken)
    {
        await authService.RevokeRefreshTokenAsync(refreshToken);
        return NoContent();
    }
}
