using AuthJWT.Api.DTOs;
using AuthJWT.Api.Models;
using AuthJWT.Api.Repositories;
using AuthJWT.Api.Services;
using Microsoft.Extensions.Configuration;
using Moq;
using FluentAssertions;

namespace AuthJWT.Api.Tests.Services;

public class AuthServiceRefreshTests
{
    private readonly Mock<IUserRepository> _userRepo;
    private readonly Mock<IRefreshTokenRepository> _refreshRepo;
    private readonly IConfiguration _config;
    private readonly AuthService _sut;

    public AuthServiceRefreshTests()
    {
        _userRepo = new Mock<IUserRepository>();
        _refreshRepo = new Mock<IRefreshTokenRepository>();
        _config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Jwt:Key"]      = "TestSecretKey_ForTests_AtLeast32Chars!!",
                ["Jwt:Issuer"]   = "TestIssuer",
                ["Jwt:Audience"] = "TestAudience"
            })
            .Build();

        _sut = new AuthService(_userRepo.Object, _refreshRepo.Object, _config);
    }

    [Fact]
    public async Task Login_ReturnsAccessTokenAndRefreshToken()
    {
        var user = new User
        {
            Id = 1,
            Name = "Ana",
            Email = "ana@email.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("senha123"),
            Role = "Employee"
        };
        _userRepo.Setup(r => r.GetByEmailAsync("ana@email.com")).ReturnsAsync(user);
        _refreshRepo.Setup(r => r.CreateAsync(It.IsAny<RefreshToken>()))
                    .ReturnsAsync((RefreshToken t) => t);

        var result = await _sut.LoginAsync(new LoginRequest("ana@email.com", "senha123"));

        result.Should().NotBeNull();
        result!.AccessToken.Should().NotBeNullOrWhiteSpace();
        result.RefreshToken.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task Refresh_WithValidToken_ReturnsNewTokenPair()
    {
        var user = new User { Id = 1, Name = "Ana", Email = "ana@email.com", Role = "Employee" };
        var existingToken = new RefreshToken
        {
            Token = "valid-token",
            UserId = 1,
            User = user,
            ExpiresAt = DateTime.UtcNow.AddDays(7)
        };

        _refreshRepo.Setup(r => r.GetByTokenAsync("valid-token")).ReturnsAsync(existingToken);
        _refreshRepo.Setup(r => r.RevokeAsync(It.IsAny<RefreshToken>())).Returns(Task.CompletedTask);
        _refreshRepo.Setup(r => r.CreateAsync(It.IsAny<RefreshToken>()))
                    .ReturnsAsync((RefreshToken t) => t);

        var result = await _sut.RefreshAsync("valid-token");

        result.Should().NotBeNull();
        result!.AccessToken.Should().NotBeNullOrWhiteSpace();
        result.RefreshToken.Should().NotBeNullOrWhiteSpace();
        result.RefreshToken.Should().NotBe("valid-token");
    }

    [Fact]
    public async Task Refresh_WithExpiredToken_ReturnsNull()
    {
        var expiredToken = new RefreshToken
        {
            Token = "expired-token",
            ExpiresAt = DateTime.UtcNow.AddDays(-1)
        };
        _refreshRepo.Setup(r => r.GetByTokenAsync("expired-token")).ReturnsAsync(expiredToken);

        var result = await _sut.RefreshAsync("expired-token");

        result.Should().BeNull();
        _refreshRepo.Verify(r => r.RevokeAsync(It.IsAny<RefreshToken>()), Times.Never);
    }

    [Fact]
    public async Task Refresh_WithRevokedToken_ReturnsNull()
    {
        var revokedToken = new RefreshToken
        {
            Token = "revoked-token",
            ExpiresAt = DateTime.UtcNow.AddDays(7),
            RevokedAt = DateTime.UtcNow.AddHours(-1)
        };
        _refreshRepo.Setup(r => r.GetByTokenAsync("revoked-token")).ReturnsAsync(revokedToken);

        var result = await _sut.RefreshAsync("revoked-token");

        result.Should().BeNull();
    }

    [Fact]
    public async Task Refresh_WithNonExistentToken_ReturnsNull()
    {
        _refreshRepo.Setup(r => r.GetByTokenAsync(It.IsAny<string>()))
                    .ReturnsAsync((RefreshToken?)null);

        var result = await _sut.RefreshAsync("inexistente");

        result.Should().BeNull();
    }

    [Fact]
    public async Task Logout_WithValidToken_RevokesAndReturnsTrue()
    {
        var token = new RefreshToken
        {
            Token = "valid-token",
            ExpiresAt = DateTime.UtcNow.AddDays(7)
        };
        _refreshRepo.Setup(r => r.GetByTokenAsync("valid-token")).ReturnsAsync(token);
        _refreshRepo.Setup(r => r.RevokeAsync(It.IsAny<RefreshToken>())).Returns(Task.CompletedTask);

        var result = await _sut.LogoutAsync("valid-token");

        result.Should().BeTrue();
        _refreshRepo.Verify(r => r.RevokeAsync(token), Times.Once);
    }

    [Fact]
    public async Task Logout_WithInvalidToken_ReturnsFalse()
    {
        _refreshRepo.Setup(r => r.GetByTokenAsync(It.IsAny<string>()))
                    .ReturnsAsync((RefreshToken?)null);

        var result = await _sut.LogoutAsync("invalido");

        result.Should().BeFalse();
    }
}
