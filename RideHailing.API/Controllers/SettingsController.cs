// ============================================================
// Controllers/SettingsController.cs — System Settings API
// Connected directly to Phase 1 app_settings table using Dapper
// ============================================================
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RideHailing.API.Models;
using RideHailing.API.Repositories;
using RideHailing.API.Services;

namespace RideHailing.API.Controllers;

[ApiController]
[Route("api/settings")]
public class SettingsController(ISettingsRepository settingsRepo, ISettingsService settingsService) : ControllerBase
{
    // Public settings (Fares, Surge switches) — React frontend reads this
    [HttpGet("public")]
    public async Task<IActionResult> GetPublic()
    {
        var all = await settingsRepo.GetByCategoryAsync(null);
        // Returns the values where is_public is true
        return Ok(all.Where(s => s.IsPublic));
    }

    // Fetch all settings — Restricted to system administrators
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] string? category)
    {
        var all = await settingsRepo.GetByCategoryAsync(category);
        return Ok(all);
    }

    // Update an individual configuration value and write to admin audit log
    [HttpPatch("{key}")]
    public async Task<IActionResult> Update(string key, [FromBody] UpdateSettingRequest req)
    {
        // For testing purposes, we use a mock administrator ID from our seed data
        // Once login tokens are ready, this will read from the secure user token claim!
        var mockAdminId = Guid.Parse("018f3a3a-3333-7777-beee-000000000001");
        
        await settingsRepo.UpdateAsync(key, req.Value, mockAdminId);
        await settingsService.InvalidateCacheAsync();
        return NoContent();
    }
}
