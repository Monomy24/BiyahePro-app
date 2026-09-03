// File path in project: RideHailing.API/Repositories/UserRepository.cs
using Dapper;
using Npgsql;
using RideHailing.API.Models;

namespace RideHailing.API.Repositories;

public interface IUserRepository
{
    Task<User?> GetByIdAsync(Guid id);
    Task<User?> GetByEmailAsync(string email);
    Task<User?> GetByPhoneAsync(string phone);
    Task<User> CreateAsync(User user);
    Task SaveRefreshTokenAsync(Guid userId, string token, DateTime expiry);
    Task<Guid?> ValidateRefreshTokenAsync(string token);
    Task RevokeRefreshTokenAsync(string token);
    Task<PagedResult<User>> GetAllAsync(string? role, int page, int pageSize);
}

public class UserRepository(IConfiguration config) : IUserRepository
{
    private NpgsqlConnection Connection() => new(config.GetConnectionString("DefaultConnection"));

    public async Task<User?> GetByIdAsync(Guid id)
    {
        using var db = Connection();
        return await db.QuerySingleOrDefaultAsync<User>("SELECT * FROM users WHERE id = @Id", new { Id = id });
    }

    public async Task<User?> GetByEmailAsync(string email)
    {
        using var db = Connection();
        return await db.QuerySingleOrDefaultAsync<User>("SELECT * FROM users WHERE email = @Email", new { Email = email });
    }

    public async Task<User?> GetByPhoneAsync(string phone)
    {
        using var db = Connection();
        return await db.QuerySingleOrDefaultAsync<User>("SELECT * FROM users WHERE phone = @Phone", new { Phone = phone });
    }

    public async Task<User> CreateAsync(User user)
    {
        using var db = Connection();
        var sql = "INSERT INTO users (full_name, email, phone, password_hash, role, is_active, is_verified) VALUES (@FullName, @Email, @Phone, @PasswordHash, @Role, @IsActive, @IsVerified) RETURNING *";
        return await db.QuerySingleAsync<User>(sql, user);
    }

    public async Task SaveRefreshTokenAsync(Guid userId, string token, DateTime expiry)
    {
        using var db = Connection();
        await db.ExecuteAsync("INSERT INTO user_refresh_tokens (user_id, token_hash, expires_at) VALUES (@UserId, @Token, @Expiry)", new { UserId = userId, Token = token, Expiry = expiry });
    }

    public async Task<Guid?> ValidateRefreshTokenAsync(string token)
    {
        using var db = Connection();
        return await db.QuerySingleOrDefaultAsync<Guid?>("SELECT user_id FROM user_refresh_tokens WHERE token_hash = @Token AND expires_at > NOW() AND revoked_at IS NULL", new { Token = token });
    }

    public async Task RevokeRefreshTokenAsync(string token)
    {
        using var db = Connection();
        await db.ExecuteAsync("UPDATE user_refresh_tokens SET revoked_at = NOW() WHERE token_hash = @Token", new { Token = token });
    }

    public async Task<PagedResult<User>> GetAllAsync(string? role, int page, int pageSize)
    {
        using var db = Connection();
        var offset = (page - 1) * pageSize;
        var where = role != null ? "WHERE role = @Role" : "";
        var total = await db.QuerySingleAsync<int>($"SELECT COUNT(*) FROM users {where}", new { Role = role });
        var items = await db.QueryAsync<User>($"SELECT * FROM users {where} ORDER BY created_at DESC LIMIT @PageSize OFFSET @Offset", new { Role = role, PageSize = pageSize, Offset = offset });
        return new PagedResult<User> { Items = items.ToList(), TotalCount = total, Page = page, PageSize = pageSize };
    }
}