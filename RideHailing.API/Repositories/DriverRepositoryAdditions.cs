// ============================================================
// Repositories/DriverRepositoryAdditions.cs
// Add these two methods to IDriverRepository and DriverRepository
// in your existing Repositories.cs file
// ============================================================
// 
// INSTRUCTION: Open your Repositories.cs and:
//
// 1. Add these two lines to the IDriverRepository interface:
//
//    Task<Driver?> GetByIdAsync(Guid driverId);
//    Task<Vehicle?> GetVehicleAsync(Guid driverId);
//
// 2. Add these two method implementations inside DriverRepository class:
//
//    public async Task<Driver?> GetByIdAsync(Guid driverId)
//    {
//        using var db = Connection();
//        return await db.QuerySingleOrDefaultAsync<Driver>("""
//            SELECT d.*, u.full_name, u.phone, u.email
//            FROM drivers d
//            JOIN users u ON u.id = d.user_id
//            WHERE d.id = @Id
//            """, new { Id = driverId });
//    }
//
//    public async Task<Vehicle?> GetVehicleAsync(Guid driverId)
//    {
//        using var db = Connection();
//        return await db.QuerySingleOrDefaultAsync<Vehicle>(
//            "SELECT * FROM vehicles WHERE driver_id = @DriverId AND is_active = true",
//            new { DriverId = driverId });
//    }
//
// ============================================================
// WHY THIS FILE EXISTS:
// Your project structure has separate .cs files per concern.
// Rather than replacing the entire Repositories.cs, this file
// shows exactly what to add so you don't lose existing code.
// ============================================================