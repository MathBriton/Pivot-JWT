using AuthJWT.Api.Models;
using FluentAssertions;

namespace AuthJWT.Api.Tests.Models;

public class RefreshTokenTests
{
    [Fact]
    public void IsActive_WhenNotExpiredAndNotRevoked_ReturnsTrue()
    {
        var token = new RefreshToken { ExpiresAt = DateTime.UtcNow.AddDays(7) };

        token.IsActive.Should().BeTrue();
    }

    [Fact]
    public void IsActive_WhenExpired_ReturnsFalse()
    {
        var token = new RefreshToken { ExpiresAt = DateTime.UtcNow.AddDays(-1) };

        token.IsActive.Should().BeFalse();
    }

    [Fact]
    public void IsActive_WhenRevoked_ReturnsFalse()
    {
        var token = new RefreshToken
        {
            ExpiresAt = DateTime.UtcNow.AddDays(7),
            RevokedAt = DateTime.UtcNow
        };

        token.IsActive.Should().BeFalse();
    }

    [Fact]
    public void IsExpired_WhenExpiresAtInPast_ReturnsTrue()
    {
        var token = new RefreshToken { ExpiresAt = DateTime.UtcNow.AddSeconds(-1) };

        token.IsExpired.Should().BeTrue();
    }

    [Fact]
    public void IsRevoked_WhenRevokedAtIsNull_ReturnsFalse()
    {
        var token = new RefreshToken { ExpiresAt = DateTime.UtcNow.AddDays(7) };

        token.IsRevoked.Should().BeFalse();
    }
}
