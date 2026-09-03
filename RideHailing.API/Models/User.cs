// File path in project: RideHailing.API/Models/User.cs
// ============================================================
// Models/User.cs
// User domain model + Auth request/response DTOs
// ============================================================
using System.ComponentModel.DataAnnotations;

namespace RideHailing.API.Models;

// ── Domain Model ──────────────────────────────────────────────
public class User
{
    public Guid     Id           { get; set; }
    public string   FullName     { get; set; } = string.Empty;
    public string   Email        { get; set; } = string.Empty;
    public string   Phone        { get; set; } = string.Empty;
    public string   PasswordHash { get; set; } = string.Empty;
    public string   Role         { get; set; } = "customer"; // customer | driver | admin
    public string?  AvatarUrl    { get; set; }
    public bool     IsActive     { get; set; } = true;
    public bool     IsVerified   { get; set; } = false;
    public DateTime CreatedAt    { get; set; }
    public DateTime UpdatedAt    { get; set; }
}

// ── Request DTOs ──────────────────────────────────────────────
public record RegisterRequest(
    string FullName,
    string Email,
    string Phone,
    // Password policy: 8+ characters, at least one uppercase letter,
    // one digit, and one symbol. [ApiController] validates this
    // automatically and returns 400 with the message below if it fails —
    // no controller/service code needs to check this manually.
    //
    // IMPORTANT: for a record's primary constructor, these attributes
    // must go directly on the parameter (as below), NOT with a
    // [property: ...] target. ASP.NET Core's record-aware model
    // validation reads metadata from the constructor parameter itself;
    // putting it on the generated property instead throws
    // InvalidOperationException at request time ("validation metadata
    // ... will be ignored ... must be associated with the constructor
    // parameter").
    [Required]
    [RegularExpression(
        @"^(?=.*[A-Z])(?=.*\d)(?=.*[^A-Za-z0-9]).{8,}$",
        ErrorMessage = "Password must be at least 8 characters and include at least one uppercase letter, one number, and one symbol.")]
    string Password,
    string Role = "customer"
);

public record LoginRequest(
    string Email,
    string Password
);

public record UpdateSettingRequest(string Value);

public record CancelTripRequest(string Reason);

public record SubmitRatingRequest(
    Guid   RatedUserId,
    int    Score,
    string? Comment
);

// ── Response DTOs ─────────────────────────────────────────────
public record AuthResponse(
    string AccessToken,
    string RefreshToken,
    string Role,
    Guid   UserId,
    string FullName
);

public record UserResponse(
    Guid    Id,
    string  FullName,
    string  Email,
    string  Phone,
    string  Role,
    string? AvatarUrl,
    bool    IsActive,
    bool    IsVerified,
    DateTime CreatedAt
);

// ── Shared Utility ────────────────────────────────────────────
public class PagedResult<T>
{
    public List<T> Items      { get; set; } = [];
    public int     TotalCount { get; set; }
    public int     Page       { get; set; }
    public int     PageSize   { get; set; }
    public int     TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
}