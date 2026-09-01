// File path in project: RideHailing.API/Repositories/TripRepository.cs
using Dapper;
using Npgsql;
using RideHailing.API.Models;

namespace RideHailing.API.Repositories;

public interface ITripRepository
{
    Task<Trip> CreateAsync(Trip trip);
    Task<Trip?> GetByIdAsync(Guid id);
    Task UpdateAsync(Trip trip);
    Task<PagedResult<Trip>> GetHistoryAsync(Guid userId, string role, int page, int pageSize);
    Task<List<Trip>> GetDueScheduledTripsAsync();
    Task ActivateScheduledTripAsync(Guid tripId);
}

public class TripRepository(IConfiguration config) : ITripRepository
{
    private NpgsqlConnection Connection() => new(config.GetConnectionString("DefaultConnection"));

    // Reused by GetByIdAsync/GetHistoryAsync/CreateAsync's RETURNING clause.
    // trips.pickup_location / dropoff_location are PostGIS GEOGRAPHY(POINT)
    // columns, not separate *_latitude/*_longitude columns — ST_Y/ST_X pull
    // the coordinates back out so Dapper can bind them onto
    // Trip.PickupLatitude/PickupLongitude/DropoffLatitude/DropoffLongitude
    // (works because Program.cs sets Dapper.DefaultTypeMap.MatchNamesWithUnderscores = true).
    private const string LatLngSelectExpr = @"
        ST_Y(pickup_location::geometry)  AS pickup_latitude,
        ST_X(pickup_location::geometry)  AS pickup_longitude,
        ST_Y(dropoff_location::geometry) AS dropoff_latitude,
        ST_X(dropoff_location::geometry) AS dropoff_longitude";

    public async Task<Trip> CreateAsync(Trip trip)
    {
        using var db = Connection();
        var sql = $@"
            INSERT INTO trips (
                customer_id, pickup_location, dropoff_location,
                pickup_address, dropoff_address, payment_method,
                base_fare, distance_fare, surge_multiplier, booking_fee, fare_amount, status, scheduled_for
            ) VALUES (
                @CustomerId,
                ST_SetSRID(ST_MakePoint(@PickupLongitude, @PickupLatitude), 4326)::geography,
                ST_SetSRID(ST_MakePoint(@DropoffLongitude, @DropoffLatitude), 4326)::geography,
                @PickupAddress, @DropoffAddress, @PaymentMethod,
                @BaseFare, @DistanceFare, @SurgeMultiplier, @BookingFee, @FareAmount, @Status, @ScheduledFor
            )
            RETURNING *, {LatLngSelectExpr}";
        return await db.QuerySingleAsync<Trip>(sql, trip);
    }

    public async Task<Trip?> GetByIdAsync(Guid id)
    {
        using var db = Connection();
        var sql = $"SELECT t.*, {LatLngSelectExpr} FROM trips t WHERE t.id = @Id";
        return await db.QuerySingleOrDefaultAsync<Trip>(sql, new { Id = id });
    }

    public async Task UpdateAsync(Trip trip)
    {
        using var db = Connection();
        // NOTE: trips has no updated_at column (per-state timestamps only —
        // see 03_trips.sql), so it's intentionally not set here.
        // Also persists fare_amount (needed for the late-cancellation fee in
        // TripService.CancelAsync) and the en_route/arrived/started
        // timestamps set by TripService.UpdateStatusAsync — both were
        // silently dropped before since this UPDATE never included them.
        var sql = @"
            UPDATE trips SET
                driver_id      = @DriverId,
                status         = @Status,
                payment_status = @PaymentStatus,
                cancelled_by   = @CancelledBy,
                cancel_reason  = @CancelReason,
                fare_amount    = @FareAmount,
                accepted_at    = @AcceptedAt,
                en_route_at    = @EnRouteAt,
                arrived_at     = @ArrivedAt,
                started_at     = @StartedAt,
                completed_at   = @CompletedAt,
                cancelled_at   = @CancelledAt
            WHERE id = @Id";
        await db.ExecuteAsync(sql, trip);
    }

    public async Task<PagedResult<Trip>> GetHistoryAsync(Guid userId, string role, int page, int pageSize)
    {
        using var db = Connection();
        var offset = (page - 1) * pageSize;
        var column = role == "driver" ? "driver_id" : "customer_id";
        var total = await db.QuerySingleAsync<int>($"SELECT COUNT(*) FROM trips WHERE {column} = @UserId", new { UserId = userId });
        var sql = $@"
            SELECT t.*, {LatLngSelectExpr},
                uc.full_name AS customer_name,
                ud.full_name AS driver_name
            FROM trips t
            LEFT JOIN users uc ON uc.id = t.customer_id
            LEFT JOIN drivers d ON d.id = t.driver_id
            LEFT JOIN users ud ON ud.id = d.user_id
            WHERE t.{column} = @UserId
            ORDER BY t.requested_at DESC
            LIMIT @PageSize OFFSET @Offset";
        var items = await db.QueryAsync<Trip>(sql, new { UserId = userId, PageSize = pageSize, Offset = offset });
        return new PagedResult<Trip> { Items = items.ToList(), TotalCount = total, Page = page, PageSize = pageSize };
    }

    // ── Scheduled rides (BP §III) ──────────────────────────────

    // Scheduled trips whose time has arrived and haven't been activated
    // yet — picked up by ScheduledTripActivationService on a timer.
    public async Task<List<Trip>> GetDueScheduledTripsAsync()
    {
        using var db = Connection();
        var sql = $@"
            SELECT t.*, {LatLngSelectExpr}
            FROM trips t
            WHERE t.status = 'scheduled' AND t.scheduled_for <= NOW()
            ORDER BY t.scheduled_for ASC";
        var result = await db.QueryAsync<Trip>(sql);
        return result.ToList();
    }

    // Flips a due scheduled trip to 'requested' so it enters the normal
    // dispatch flow — doesn't touch accepted_at/etc, just opens it up
    // for driver matching exactly like a fresh immediate booking.
    public async Task ActivateScheduledTripAsync(Guid tripId)
    {
        using var db = Connection();
        await db.ExecuteAsync(
            "UPDATE trips SET status = 'requested' WHERE id = @Id AND status = 'scheduled'",
            new { Id = tripId });
    }
}