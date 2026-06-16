// ============================================================
// Models/Driver.cs
// Driver + Vehicle domain models and related DTOs
// ============================================================
namespace RideHailing.API.Models;

// ── Domain Models ─────────────────────────────────────────────
public class Driver
{
    public Guid     Id                    { get; set; }
    public Guid     UserId                { get; set; }
    public string   LicenseNumber         { get; set; } = string.Empty;
    public DateOnly LicenseExpiry         { get; set; }

    // offline | available | on_trip | suspended
    public string   Status                { get; set; } = "offline";

    // Unpacked from PostGIS GEOGRAPHY column in repository
    public double?  Latitude              { get; set; }
    public double?  Longitude             { get; set; }

    public decimal  Rating                { get; set; } = 5.00m;
    public int      TotalTrips            { get; set; }
    public bool     IsDocumentsVerified   { get; set; }
    public DateTime CreatedAt             { get; set; }
    public DateTime UpdatedAt             { get; set; }

    // ── Joined from users table ───────────────────────────────
    public string?  FullName              { get; set; }
    public string?  Phone                 { get; set; }
    public string?  Email                 { get; set; }

    // ── Joined from vehicles table ────────────────────────────
    public Vehicle? Vehicle               { get; set; }
}

public class Vehicle
{
    public Guid    Id           { get; set; }
    public Guid    DriverId     { get; set; }
    public string  PlateNumber  { get; set; } = string.Empty;
    public string  Make         { get; set; } = string.Empty; // e.g. Toyota
    public string  Model        { get; set; } = string.Empty; // e.g. Vios
    public string  Color        { get; set; } = string.Empty;
    public short   Year         { get; set; }

    // sedan | suv | van | motorcycle
    public string  VehicleType  { get; set; } = "sedan";
    public bool    IsActive     { get; set; } = true;
    public DateTime CreatedAt   { get; set; }
}

// ── Request DTOs ──────────────────────────────────────────────
public record DriverLocationUpdate(
    Guid   DriverId,
    double Latitude,
    double Longitude
);

public record RegisterDriverRequest(
    // Inherits user fields
    string   FullName,
    string   Email,
    string   Phone,
    string   Password,
    // Driver-specific
    string   LicenseNumber,
    DateOnly LicenseExpiry,
    // Vehicle
    string   PlateNumber,
    string   Make,
    string   Model,
    string   Color,
    short    Year,
    string   VehicleType = "sedan"
);

// ── Response DTOs ─────────────────────────────────────────────
public record NearbyDriverResponse(
    Guid    DriverId,
    string  FullName,
    decimal Rating,
    double  Latitude,
    double  Longitude,
    double  DistanceKm,
    string  PlateNumber,
    string  VehicleModel,
    string  VehicleColor
);

public record DriverStatusResponse(
    Guid    DriverId,
    string  Status,
    double? Latitude,
    double? Longitude,
    decimal Rating,
    int     TotalTrips
);