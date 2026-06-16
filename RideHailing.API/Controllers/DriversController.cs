// ============================================================
// Controllers/DriversController.cs — Fleet Tracking API
// Allows drivers to view profiles and admins to track statuses
// ============================================================
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RideHailing.API.Repositories;
using RideHailing.API.Services;

namespace RideHailing.API.Controllers;

[ApiController]
[Route("api/drivers")]
[Authorize]
public class DriversController(IDriverRepository driverRepo, IDriverService driverService) : ControllerBase
{
    // GET: api/drivers/me (Allows an authenticated driver to view their metrics)
    [HttpGet("me")]
    [Authorize(Roles = "driver")]
    public async Task<IActionResult> GetMyProfile()
    {
        var claimId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (claimId == null) return Unauthorized();

        var userId = Guid.Parse(claimId);
        var driver = await driverService.GetProfileAsync(userId);
        if (driver == null) return NotFound(new { message = "Driver profile card not found." });
        
        return Ok(driver);
    }

    // GET: api/drivers (Admin-only panel to review the entire fleet status)
    [HttpGet]
    [Authorize(Roles = "admin")]
    public async Task<IActionResult> GetAll(
        [FromQuery] string? status,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        var result = await driverService.GetAllAsync(status, page, pageSize);
        return Ok(result);
    }
}
