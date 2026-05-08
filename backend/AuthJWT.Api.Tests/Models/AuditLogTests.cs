using AuthJWT.Api.Models;
using FluentAssertions;

namespace AuthJWT.Api.Tests.Models;

public class AuditLogTests
{
    [Fact]
    public void AuditLog_OccurredAt_IsSetAutomatically()
    {
        var before = DateTime.UtcNow.AddSeconds(-1);

        var log = new AuditLog
        {
            UserName = "Ana",
            Action = AuditAction.CustomerCreated,
            EntityName = "Customer"
        };

        log.OccurredAt.Should().BeAfter(before);
    }

    [Fact]
    public void AuditLog_WithNullUserId_IsValid()
    {
        var log = new AuditLog
        {
            UserName = "anonymous",
            Action = AuditAction.AuthLoginFailed,
            EntityName = "Auth"
        };

        log.UserId.Should().BeNull();
        log.UserName.Should().Be("anonymous");
    }

    [Fact]
    public void AuditLog_AllActionConstants_AreNonEmpty()
    {
        var actions = new[]
        {
            AuditAction.AuthLogin,
            AuditAction.AuthLoginFailed,
            AuditAction.AuthRegister,
            AuditAction.AuthLogout,
            AuditAction.AuthRefreshToken,
            AuditAction.CustomerCreated,
            AuditAction.CustomerUpdated,
            AuditAction.CustomerDeleted,
            AuditAction.WorkTaskCreated,
            AuditAction.WorkTaskUpdated,
            AuditAction.WorkTaskStatusChanged
        };

        actions.Should().AllSatisfy(a => a.Should().NotBeNullOrWhiteSpace());
    }
}
