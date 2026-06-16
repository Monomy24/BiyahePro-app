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
}

public class TripRepository(IConfiguration config) : ITripRepository
{
    private NpgsqlConnection Connection() => new(config.GetConnectionString("DefaultConnection"));

    public async Task<Trip> CreateAsync(Trip trip)
    {
        using var db = Connection();
        var sql = "INSERT INTO trips (customer_id, pickup_latitude, pickup_longitude, dropoff_latitude, dropoff_longitude, pickup_address, dropoff_address, payment_method, base_fare, distance_fare, surge_multiplier, fare_amount, status) VALUES (@CustomerId, @PickupLatitude, @PickupLongitude, @DropoffLatitude, @DropoffLongitude, @PickupAddress, @DropoffAddress, @PaymentMethod, @BaseFare, @DistanceFare, @SurgeMultiplier, @FareAmount, @Status) RETURNING *";
        return await db.QuerySingleAsync<Trip>(sql, trip);
    }

    public async Task<Trip?> GetByIdAsync(Guid id)
    {
        using var db = Connection();
        return await db.QuerySingleOrDefaultAsync<Trip>("SELECT * FROM trips WHERE id = @Id", new { Id = id });
    }

    public async Task UpdateAsync(Trip trip)
    {
        using var db = Connection();
        var sql = "UPDATE trips SET driver_id = @DriverId, status = @Status, payment_status = @PaymentStatus, cancelled_by = @CancelledBy, cancel_reason = @CancelReason, accepted_at = @AcceptedAt, completed_at = @CompletedAt, cancelled_at = @CancelledAt, updated_at = NOW() WHERE id = @Id";
        await db.ExecuteAsync(sql, trip);
    }

    public async Task<PagedResult<Trip>> GetHistoryAsync(Guid userId, string role, int page, int pageSize)
    {
        using var db = Connection();
        var offset = (page - 1) * pageSize;
        var column = role == "driver" ? "driver_id" : "customer_id";
        var total = await db.QuerySingleAsync<int>($"SELECT COUNT(*) FROM trips WHERE {column} = @UserId", new { UserId = userId });
        var items = await db.QueryAsync<Trip>($"SELECT t.*, uc.full_name as CustomerName, ud.full_name as DriverName FROM trips t LEFT JOIN users uc ON uc.id = t.customer_id LEFT JOIN drivers d ON d.id = t.driver_id LEFT JOIN users ud ON ud.id = d.user_id WHERE t.{column} = @UserId ORDER BY t.requested_at DESC LIMIT @PageSize OFFSET @Offset", new { UserId = userId, PageSize = pageSize, Offset = offset });
        return new PagedResult<Trip> { Items = items.ToList(), TotalCount = total, Page = page, PageSize = pageSize };
    }
}
