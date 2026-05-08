using AuthJWT.Api.DTOs;
using AuthJWT.Api.Models;
using AuthJWT.Api.Repositories;
using AuthJWT.Api.Services;
using Microsoft.Extensions.Configuration;
using Moq;

namespace AuthJWT.Api.Tests.Services;

public class AuthServiceTests
{
    private readonly Mock<IUserRepository> _userRepo;
    private readonly Mock<IRefreshTokenRepository> _refreshRepo;
    private readonly IConfiguration _config;
    private readonly AuthService _sut;

    public AuthServiceTests()
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

    // --- Register ---

    [Fact]
    public async Task Register_WithValidData_ReturnsTokenAndUserInfo()
    {
        _userRepo.Setup(r => r.EmailExistsAsync("novo@email.com")).ReturnsAsync(false);
        _userRepo.Setup(r => r.CreateAsync(It.IsAny<User>()))
                 .ReturnsAsync((User u) => { u.Id = 1; return u; });
        _refreshRepo.Setup(r => r.CreateAsync(It.IsAny<RefreshToken>()))
                    .ReturnsAsync((RefreshToken t) => t);

        var result = await _sut.RegisterAsync(new RegisterRequest("Novo", "novo@email.com", "senha123"));

        Assert.NotNull(result);
        Assert.Equal("Novo", result.Name);
        Assert.Equal("novo@email.com", result.Email);
        Assert.Equal("Employee", result.Role);
        Assert.False(string.IsNullOrWhiteSpace(result.AccessToken));
    }

    [Fact]
    public async Task Register_WithDuplicateEmail_ReturnsNull()
    {
        _userRepo.Setup(r => r.EmailExistsAsync("existente@email.com")).ReturnsAsync(true);

        var result = await _sut.RegisterAsync(new RegisterRequest("Alguém", "existente@email.com", "senha123"));

        Assert.Null(result);
        _userRepo.Verify(r => r.CreateAsync(It.IsAny<User>()), Times.Never);
    }

    [Fact]
    public async Task Register_SavesToRepository_WithHashedPassword()
    {
        User? savedUser = null;
        _userRepo.Setup(r => r.EmailExistsAsync(It.IsAny<string>())).ReturnsAsync(false);
        _userRepo.Setup(r => r.CreateAsync(It.IsAny<User>()))
                 .Callback<User>(u => savedUser = u)
                 .ReturnsAsync((User u) => u);
        _refreshRepo.Setup(r => r.CreateAsync(It.IsAny<RefreshToken>()))
                    .ReturnsAsync((RefreshToken t) => t);

        await _sut.RegisterAsync(new RegisterRequest("User", "user@email.com", "minhasenha"));

        Assert.NotNull(savedUser);
        Assert.NotEqual("minhasenha", savedUser.PasswordHash);
        Assert.True(BCrypt.Net.BCrypt.Verify("minhasenha", savedUser.PasswordHash));
    }

    [Fact]
    public async Task Register_NormalizesEmailToLowercase()
    {
        User? savedUser = null;
        _userRepo.Setup(r => r.EmailExistsAsync(It.IsAny<string>())).ReturnsAsync(false);
        _userRepo.Setup(r => r.CreateAsync(It.IsAny<User>()))
                 .Callback<User>(u => savedUser = u)
                 .ReturnsAsync((User u) => u);
        _refreshRepo.Setup(r => r.CreateAsync(It.IsAny<RefreshToken>()))
                    .ReturnsAsync((RefreshToken t) => t);

        await _sut.RegisterAsync(new RegisterRequest("User", "UPPER@EMAIL.COM", "senha123"));

        Assert.Equal("upper@email.com", savedUser?.Email);
    }

    // --- Login ---

    [Fact]
    public async Task Login_WithValidCredentials_ReturnsToken()
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

        Assert.NotNull(result);
        Assert.Equal("Ana", result.Name);
        Assert.False(string.IsNullOrWhiteSpace(result.AccessToken));
    }

    [Fact]
    public async Task Login_WithWrongPassword_ReturnsNull()
    {
        var user = new User
        {
            Email = "ana@email.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("senha123")
        };
        _userRepo.Setup(r => r.GetByEmailAsync("ana@email.com")).ReturnsAsync(user);

        var result = await _sut.LoginAsync(new LoginRequest("ana@email.com", "senhaErrada"));

        Assert.Null(result);
    }

    [Fact]
    public async Task Login_WithNonExistentEmail_ReturnsNull()
    {
        _userRepo.Setup(r => r.GetByEmailAsync(It.IsAny<string>())).ReturnsAsync((User?)null);

        var result = await _sut.LoginAsync(new LoginRequest("naoexiste@email.com", "senha123"));

        Assert.Null(result);
    }
}
