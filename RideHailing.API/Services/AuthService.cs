using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using RideHailing.API.Models;
using RideHailing.API.Repositories;

namespace RideHailing.API.Services;

public interface IAuthService
{
    Task<AuthResponse?> RegisterAsync(RegisterRequest request);
    Task<AuthResponse?> LoginAsync(LoginRequest request);
    Task<AuthResponse?> RefreshTokenAsync(string refreshToken);
    Task RevokeRefreshTokenAsync(string refreshToken);
}

public class AuthService(
    IUserRepository userRepo,
    IConfiguration config) : IAuthService
{
    public async Task<AuthResponse?> RegisterAsync(RegisterRequest request)
    {
        var existing = await userRepo.GetByEmailAsync(request.Email);
        if (existing != null) return null;

        var user = new User
        {
            Id = Guid.NewGuid(),
            FullName = request.FullName,
            Email = request.Email.ToLowerInvariant(),
            Phone = request.Phone,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            Role = request.Role,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await userRepo.CreateAsync(user);
        return BuildAuthResponse(user);
    }

    public async Task<AuthResponse?> LoginAsync(LoginRequest request)
    {
        var user = await userRepo.GetByEmailAsync(request.Email.ToLowerInvariant());
        if (user == null || !user.IsActive) return null;
        // Guard against a null/empty hash (e.g. bad mapping, corrupted row) so
        // this fails as "invalid credentials" instead of throwing a 500 —
        // BCrypt.Verify throws on a null/malformed hash rather than returning false.
        if (string.IsNullOrEmpty(user.PasswordHash)) return null;
        if (!BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash)) return null;

        var refreshToken = GenerateRefreshToken();
        var expiryDays = config.GetValue<int>("Jwt:RefreshTokenExpiryDays", 7);
        await userRepo.SaveRefreshTokenAsync(user.Id, refreshToken, DateTime.UtcNow.AddDays(expiryDays));

        return BuildAuthResponse(user, refreshToken);
    }

    public async Task<AuthResponse?> RefreshTokenAsync(string refreshToken)
    {
        var userId = await userRepo.ValidateRefreshTokenAsync(refreshToken);
        if (userId == null) return null;

        var user = await userRepo.GetByIdAsync(userId.Value);
        if (user == null || !user.IsActive) return null;

        var newRefreshToken = GenerateRefreshToken();
        var expiryDays = config.GetValue<int>("Jwt:RefreshTokenExpiryDays", 7);
        await userRepo.RevokeRefreshTokenAsync(refreshToken);
        await userRepo.SaveRefreshTokenAsync(user.Id, newRefreshToken, DateTime.UtcNow.AddDays(expiryDays));

        return BuildAuthResponse(user, newRefreshToken);
    }

    public async Task RevokeRefreshTokenAsync(string refreshToken)
        => await userRepo.RevokeRefreshTokenAsync(refreshToken);

    private AuthResponse BuildAuthResponse(User user, string? refreshToken = null)
    {
        var accessToken = GenerateJwt(user);
        refreshToken ??= GenerateRefreshToken();
        return new AuthResponse(accessToken, refreshToken, user.Role, user.Id, user.FullName);
    }

    private string GenerateJwt(User user)
    {
        var secret = config["Jwt:Secret"] ?? throw new InvalidOperationException("JWT secret missing");
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var expiry = config.GetValue<int>("Jwt:AccessTokenExpiryMinutes", 15);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim(ClaimTypes.Role, user.Role),
            new Claim("full_name", user.FullName),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var token = new JwtSecurityToken(
            issuer: config["Jwt:Issuer"],
            audience: config["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(expiry),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private static string GenerateRefreshToken()
    {
        var bytes = RandomNumberGenerator.GetBytes(64);
        return Convert.ToBase64String(bytes);
    }
}