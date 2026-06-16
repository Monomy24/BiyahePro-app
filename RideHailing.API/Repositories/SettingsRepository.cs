using Dapper;
using Npgsql;
using RideHailing.API.Models;

namespace RideHailing.API.Repositories;

public interface ISettingsRepository
{
    Task<AppSetting?> GetByKeyAsync(string key);
    Task<IEnumerable<AppSetting>> GetByCategoryAsync(string? category);
    Task UpdateAsync(string key, string value, Guid adminId);
}

public class SettingsRepository(IConfiguration config) : ISettingsRepository
{
    private NpgsqlConnection Connection() => new(config.GetConnectionString("DefaultConnection"));

    public async Task<AppSetting?> GetByKeyAsync(string key)
    {
        using var db = Connection();
        return await db.QuerySingleOrDefaultAsync<AppSetting>("SELECT id, key, value, data_type, category, label, description, is_public FROM app_settings WHERE key = @Key", new { Key = key });
    }

    public async Task<IEnumerable<AppSetting>> GetByCategoryAsync(string? category)
    {
        using var db = Connection();
        if (category == null)
            return await db.QueryAsync<AppSetting>("SELECT id, key, value, data_type, category, label, description, is_public FROM app_settings");
        return await db.QueryAsync<AppSetting>("SELECT id, key, value, data_type, category, label, description, is_public FROM app_settings WHERE category = @Category", new { Category = category });
    }

    public async Task UpdateAsync(string key, string value, Guid adminId)
    {
        using var db = Connection();
        await db.ExecuteAsync("UPDATE app_settings SET value = @Value, updated_at = NOW() WHERE key = @Key; INSERT INTO admin_audit_log (admin_id, action, target_table, target_id, old_values, new_values) VALUES (@AdminId, 'update_setting', 'app_settings', @Key, 'UNKNOWN', @Value);", new { Key = key, Value = value, AdminId = adminId });
    }
}
