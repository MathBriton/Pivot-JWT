using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using AuthJWT.Api.DTOs;
using AuthJWT.Api.Models;
using AuthJWT.Api.Repositories;
using Microsoft.IdentityModel.Tokens;

namespace AuthJWT.Api.Services;

public class AuthService(
    IUserRepository userRepo,
    IRefreshTokenRepository refreshRepo,
    IConfiguration config) : IAuthService
{
    private static readonly TimeSpan RefreshTokenLifetime = TimeSpan.FromDays(7);

    public async Task<AuthResponse?> RegisterAsync(RegisterRequest request)
    {
        if (await userRepo.EmailExistsAsync(request.Email))
            return null;

        var user = new User
        {
            Name = request.Name,
            Email = request.Email.ToLower(),
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            Role = UserRoles.Employee
        };

        await userRepo.CreateAsync(user);
        return await BuildAuthResponseAsync(user);
    }

    public async Task<AuthResponse?> LoginAsync(LoginRequest request)
    {
        var user = await userRepo.GetByEmailAsync(request.Email);
        if (user is null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            return null;

        return await BuildAuthResponseAsync(user);
    }

    public async Task<AuthResponse?> RefreshAsync(string refreshToken)
    {
        var stored = await refreshRepo.GetByTokenAsync(refreshToken);
        if (stored is null || !stored.IsActive)
            return null;

        await refreshRepo.RevokeAsync(stored);
        return await BuildAuthResponseAsync(stored.User);
    }

    public async Task<bool> LogoutAsync(string refreshToken)
    {
        var stored = await refreshRepo.GetByTokenAsync(refreshToken);
        if (stored is null)
            return false;

        await refreshRepo.RevokeAsync(stored);
        return true;
    }

    private async Task<AuthResponse> BuildAuthResponseAsync(User user)
    {
        var accessToken = GenerateAccessToken(user);
        var refreshToken = await CreateRefreshTokenAsync(user.Id);
        return new AuthResponse(accessToken, refreshToken, user.Name, user.Email, user.Role);
    }

    private async Task<string> CreateRefreshTokenAsync(int userId)
    {
        var token = new RefreshToken
        {
            Token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64)),
            UserId = userId,
            ExpiresAt = DateTime.UtcNow.Add(RefreshTokenLifetime)
        };
        await refreshRepo.CreateAsync(token);
        return token.Token;
    }

    private string GenerateAccessToken(User user)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["Jwt:Key"]!));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.Name),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Role, user.Role)
        };

        var token = new JwtSecurityToken(
            issuer: config["Jwt:Issuer"],
            audience: config["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddHours(8),
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
