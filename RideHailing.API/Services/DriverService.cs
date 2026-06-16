// ============================================================
// Services/DriverService.cs
// Driver assignment, availability, and management logic
// Separate from TripService to keep each service focused
// ============================================================
using RideHailing.API.Models;
using RideHailing.API.Repositories;

namespace RideHailing.API.Services;

public interface IDriverService
{
    Task<Driver?>                    GetProfileAsync(Guid userId);
    Task<DriverStatusResponse?>      GetStatusAsync(Guid driverId);
    Task<bool>                       SetAvailabilityAsync(Guid userId, bool available);
    Task<bool>                       SuspendAsync(Guid driverId, Guid adminId);
    Task<bool>                       ReinstateAsync(Guid driverId, Guid adminId);
    Task<PagedResult<Driver>>        GetAllAsync(string? status, int page, int pageSize);
    Task<List<NearbyDriverResponse>> GetNearbyAsync(double lat, double lng);
}

public class DriverService(
    IDriverRepository  driverRepo,
    ISettingsService   settings) : IDriverService
{
    // ── Profile ───────────────────────────────────────────────
    public async Task<Driver?> GetProfileAsync(Guid userId)
    {
        var driver = await driverRepo.GetByUserIdAsync(userId);
        if (driver == null) return null;

        // Attach vehicle to the driver object
        driver.Vehicle = await driverRepo.GetVehicleAsync(driver.Id);
        return driver;
    }

    public async Task<DriverStatusResponse?> GetStatusAsync(Guid driverId)
    {
        var driver = await driverRepo.GetByIdAsync(driverId);
        if (driver == null) return null;

        return new DriverStatusResponse(
            DriverId:   driver.Id,
            Status:     driver.Status,
            Latitude:   driver.Latitude,
            Longitude:  driver.Longitude,
            Rating:     driver.Rating,
            TotalTrips: driver.TotalTrips
        );
    }

    // ── Availability toggle ───────────────────────────────────
    public async Task<bool> SetAvailabilityAsync(Guid userId, bool available)
    {
        var driver = await driverRepo.GetByUserIdAsync(userId);
        if (driver == null) return false;

        // Can't go available if suspended
        if (driver.Status == "suspended") return false;

        var newStatus = available ? "available" : "offline";
        await driverRepo.UpdateStatusAsync(driver.Id, newStatus);
        return true;
    }

    // ── Admin actions ─────────────────────────────────────────
    public async Task<bool> SuspendAsync(Guid driverId, Guid adminId)
    {
        var driver = await driverRepo.GetByIdAsync(driverId);
        if (driver == null) return false;

        await driverRepo.UpdateStatusAsync(driverId, "suspended");
        return true;
    }

    public async Task<bool> ReinstateAsync(Guid driverId, Guid adminId)
    {
        var driver = await driverRepo.GetByIdAsync(driverId);
        if (driver == null || driver.Status != "suspended") return false;

        await driverRepo.UpdateStatusAsync(driverId, "offline");
        return true;
    }

    // ── Queries ───────────────────────────────────────────────
    public async Task<PagedResult<Driver>> GetAllAsync(string? status, int page, int pageSize)
        => await driverRepo.GetAllAsync(status, page, pageSize);

    public async Task<List<NearbyDriverResponse>> GetNearbyAsync(double lat, double lng)
    {
        var radiusKm = await settings.GetIntAsync(SettingKeys.OpsDriverRadius, 5);
        return await driverRepo.GetNearbyAsync(lat, lng, radiusKm);
    }
}