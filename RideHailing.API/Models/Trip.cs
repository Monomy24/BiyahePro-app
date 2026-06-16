// ============================================================
// Models/Trip.cs
// Trip domain model + booking/fare request & response DTOs
// ============================================================
namespace RideHailing.API.Models;

// ── Domain Model ──────────────────────────────────────────────
public class Trip
{
    public Guid     Id               { get; set; }
    public Guid     CustomerId       { get; set; }
    public Guid?    DriverId         { get; set; }   // null until a driver accepts

    // Unpacked from PostGIS GEOGRAPHY columns in repository
    public double   PickupLatitude   { get; set; }
    public double   PickupLongitude  { get; set; }
    public double   DropoffLatitude  { get; set; }
    public double   DropoffLongitude { get; set; }

    public string   PickupAddress    { get; set; } = string.Empty;
    public string   DropoffAddress   { get; set; } = string.Empty;

    // ── Trip state machine ────────────────────────────────────
    // requested → accepted → en_route → arrived → in_progress → completed | cancelled
    public string   Status           { get; set; } = "requested";

    // ── Fare snapshot (locked at booking time) ────────────────
    public decimal  BaseFare         { get; set; }
    public decimal  DistanceFare     { get; set; }
    public decimal  TimeFare         { get; set; }
    public decimal  SurgeMultiplier  { get; set; } = 1.00m;
    public decimal  FareAmount       { get; set; }   // final total

    // ── Trip metrics (filled on completion) ──────────────────
    public decimal? DistanceKm       { get; set; }
    public int?     DurationMinutes  { get; set; }

    // ── Payment ───────────────────────────────────────────────
    // cash | gcash | card
    public string   PaymentMethod    { get; set; } = "cash";
    // pending | paid | refunded
    public string   PaymentStatus    { get; set; } = "pending";

    // ── Cancellation ──────────────────────────────────────────
    public string?  CancelledBy      { get; set; }  // customer | driver | system
    public string?  CancelReason     { get; set; }

    // ── State timestamps ──────────────────────────────────────
    public DateTime  RequestedAt     { get; set; }
    public DateTime? AcceptedAt      { get; set; }
    public DateTime? EnRouteAt       { get; set; }
    public DateTime? ArrivedAt       { get; set; }
    public DateTime? StartedAt       { get; set; }
    public DateTime? CompletedAt     { get; set; }
    public DateTime? CancelledAt     { get; set; }

    // ── Joined fields (from queries) ──────────────────────────
    public string?  CustomerName     { get; set; }
    public string?  CustomerPhone    { get; set; }
    public string?  DriverName       { get; set; }
    public string?  DriverPhone      { get; set; }
    public string?  PlateNumber      { get; set; }
}

// ── Request DTOs ──────────────────────────────────────────────
public record BookTripRequest(
    double PickupLatitude,
    double PickupLongitude,
    double DropoffLatitude,
    double DropoffLongitude,
    string PickupAddress,
    string DropoffAddress,
    string PaymentMethod = "cash"
);

public record FareEstimateRequest(
    double PickupLatitude,
    double PickupLongitude,
    double DropoffLatitude,
    double DropoffLongitude
);

public record TripStatusUpdate(
    Guid   TripId,
    string Status
);

// ── Response DTOs ─────────────────────────────────────────────
public record FareEstimateResponse(
    decimal BaseFare,
    decimal EstimatedDistanceFare,
    decimal EstimatedTotal,
    decimal SurgeMultiplier,
    double  EstimatedDistanceKm,
    int     EstimatedMinutes
);

public record TripSummaryResponse(
    Guid    Id,
    string  PickupAddress,
    string  DropoffAddress,
    string  Status,
    decimal FareAmount,
    string  PaymentMethod,
    string? DriverName,
    string? PlateNumber,
    DateTime RequestedAt,
    DateTime? CompletedAt
);