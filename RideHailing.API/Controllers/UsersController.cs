// ============================================================
// Controllers/UsersController.cs — Profile Management
// Allows clients to fetch their data and admins to audit records
// ============================================================
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RideHailing.API.Models;
using RideHailing.API.Repositories;

namespace RideHailing.API.Controllers;

[ApiController]
[Route("api/users")]
[Authorize]
public class UsersController(IUserRepository userRepo) : ControllerBase
{
    // GET: api/users/me (Fetches the logged-in user's profile card)
    [HttpGet("me")]
    public async Task<IActionResult> GetMe()
    {
        var claimId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (claimId == null) return Unauthorized();

        var id = Guid.Parse(claimId);
        var user = await userRepo.GetByIdAsync(id);
        if (user == null) return NotFound(new { message = "User record not found." });
        
        return Ok(new UserResponse(
            user.Id, user.FullName, user.Email, user.Phone,
            user.Role, user.AvatarUrl, user.IsActive, user.IsVerified, user.CreatedAt
        ));
    }

    // GET: api/users (Admin-only list to scan registered profiles)
    [HttpGet]
    [Authorize(Roles = "admin")]
    public async Task<IActionResult> GetAll(
        [FromQuery] string? role,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        var result = await userRepo.GetAllAsync(role, page, pageSize);
        return Ok(result);
    }
}
