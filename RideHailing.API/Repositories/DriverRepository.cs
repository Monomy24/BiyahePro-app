using Dapper;
using Npgsql;
using RideHailing.API.Models;

namespace RideHailing.API.Repositories;

public interface IDriverRepository
{
    Task<Driver?> GetByUserIdAsync(Guid userId);
    Task<Driver?> GetByIdAsync(Guid id);
    Task<Vehicle?> GetVehicleAsync(Guid driverId);
    Task UpdateStatusAsync(Guid driverId, string status);
    Task UpdateLocationAsync(Guid driverId, double lat, double lng);
    Task<List<NearbyDriverResponse>> GetNearbyAsync(double lat, double lng, int radiusKm);
    Task<PagedResult<Driver>> GetAllAsync(string? status, int page, int pageSize);
}

public class DriverRepository(IConfiguration config) : IDriverRepository
{
    private NpgsqlConnection Connection() => new(config.GetConnectionString("DefaultConnection"));

    public async Task<Driver?> GetByUserIdAsync(Guid userId)
    {
        using var db = Connection();
        return await db.QuerySingleOrDefaultAsync<Driver>("SELECT d.*, u.full_name, u.phone, u.email FROM drivers d JOIN users u ON u.id = d.user_id WHERE d.user_id = @UserId", new { UserId = userId });
    }

    public async Task<Driver?> GetByIdAsync(Guid id)
    {
        using var db = Connection();
        return await db.QuerySingleOrDefaultAsync<Driver>("SELECT d.*, u.full_name, u.phone, u.email FROM drivers d JOIN users u ON u.id = d.user_id WHERE d.id = @Id", new { Id = id });
    }

    public async Task<Vehicle?> GetVehicleAsync(Guid driverId)
    {
        using var db = Connection();
        return await db.QuerySingleOrDefaultAsync<Vehicle>("SELECT * FROM vehicles WHERE driver_id = @DriverId AND is_active = true", new { DriverId = driverId });
    }

    public async Task UpdateStatusAsync(Guid driverId, string status)
    {
        using var db = Connection();
        await db.ExecuteAsync("UPDATE drivers SET status = @Status, updated_at = NOW() WHERE id = @Id", new { Status = status, Id = driverId });
    }

    public async Task UpdateLocationAsync(Guid driverId, double lat, double lng)
    {
        using var db = Connection();
        await db.ExecuteAsync("UPDATE drivers SET current_location = ST_SetSRID(ST_MakePoint(@Lng, @Lat), 4326)::geography, updated_at = NOW() WHERE id = @Id", new { Lat = lat, Lng = lng, Id = driverId });
    }

    public async Task<List<NearbyDriverResponse>> GetNearbyAsync(double lat, double lng, int radiusKm)
    {
        using var db = Connection();
        var sql = @"SELECT d.id AS DriverId, u.full_name AS FullName, d.rating AS Rating, ST_Y(d.current_location::geometry) AS Latitude, ST_X(d.current_location::geometry) AS Longitude, ROUND((ST_Distance(d.current_location, ST_SetSRID(ST_MakePoint(@Lng, @Lat), 4326)::geography) / 1000)::numeric, 2) AS DistanceKm, v.plate_number AS PlateNumber, v.model AS VehicleModel, v.color AS VehicleColor FROM drivers d JOIN users u ON u.id = d.user_id JOIN vehicles v ON v.driver_id = d.id WHERE d.status = 'available' AND d.current_location IS NOT NULL AND ST_DWithin(d.current_location, ST_SetSRID(ST_MakePoint(@Lng, @Lat), 4326)::geography, @RadiusMeters) ORDER BY DistanceKm ASC LIMIT 10";
        var result = await db.QueryAsync<NearbyDriverResponse>(sql, new { Lat = lat, Lng = lng, RadiusMeters = radiusKm * 1000 });
        return result.ToList();
    }

    public async Task<PagedResult<Driver>> GetAllAsync(string? status, int page, int pageSize)
    {
        using var db = Connection();
        var offset = (page - 1) * pageSize;
        var where = status != null ? "WHERE d.status = @Status" : "";
        var total = await db.QuerySingleAsync<int>($"SELECT COUNT(*) FROM drivers d {where}", new { Status = status });
        var items = await db.QueryAsync<Driver>($"SELECT d.*, u.full_name, u.phone, u.email FROM drivers d JOIN users u ON u.id = d.user_id {where} ORDER BY d.created_at DESC LIMIT @PageSize OFFSET @Offset", new { Status = status, PageSize = pageSize, Offset = offset });
        return new PagedResult<Driver> { Items = items.ToList(), TotalCount = total, Page = page, PageSize = pageSize };
    }
}
