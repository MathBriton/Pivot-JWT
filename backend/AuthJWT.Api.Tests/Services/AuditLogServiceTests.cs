using AuthJWT.Api.DTOs;
using AuthJWT.Api.Models;
using AuthJWT.Api.Repositories;
using AuthJWT.Api.Services;
using FluentAssertions;
using Moq;

namespace AuthJWT.Api.Tests.Services;

public class AuditLogServiceTests
{
    private readonly Mock<IAuditLogRepository> _repo;
    private readonly AuditLogService _sut;

    public AuditLogServiceTests()
    {
        _repo = new Mock<IAuditLogRepository>();
        _sut = new AuditLogService(_repo.Object);
    }

    // --- LogAsync ---

    [Fact]
    public async Task LogAsync_CreatesLogWithCorrectFields()
    {
        AuditLog? saved = null;
        _repo.Setup(r => r.AddAsync(It.IsAny<AuditLog>()))
             .Callback<AuditLog>(l => saved = l)
             .Returns(Task.CompletedTask);

        await _sut.LogAsync(42, "Ana", AuditAction.CustomerCreated, "Customer", "1", "Detalhes");

        saved.Should().NotBeNull();
        saved!.UserId.Should().Be(42);
        saved.UserName.Should().Be("Ana");
        saved.Action.Should().Be(AuditAction.CustomerCreated);
        saved.EntityName.Should().Be("Customer");
        saved.EntityId.Should().Be("1");
        saved.Details.Should().Be("Detalhes");
    }

    [Fact]
    public async Task LogAsync_WithNullUserId_StillLogsEntry()
    {
        AuditLog? saved = null;
        _repo.Setup(r => r.AddAsync(It.IsAny<AuditLog>()))
             .Callback<AuditLog>(l => saved = l)
             .Returns(Task.CompletedTask);

        await _sut.LogAsync(null, "anonymous", AuditAction.AuthLoginFailed, "Auth");

        saved.Should().NotBeNull();
        saved!.UserId.Should().BeNull();
        saved.UserName.Should().Be("anonymous");
    }

    [Fact]
    public async Task LogAsync_WithoutOptionalFields_LogsSuccessfully()
    {
        _repo.Setup(r => r.AddAsync(It.IsAny<AuditLog>())).Returns(Task.CompletedTask);

        var act = async () => await _sut.LogAsync(1, "Ana", AuditAction.AuthLogin, "Auth");

        await act.Should().NotThrowAsync();
        _repo.Verify(r => r.AddAsync(It.IsAny<AuditLog>()), Times.Once);
    }

    // --- GetAllAsync ---

    [Fact]
    public async Task GetAll_ReturnsPaged()
    {
        var logs = new List<AuditLog>
        {
            new() { UserName = "Ana", Action = AuditAction.CustomerCreated, EntityName = "Customer" },
            new() { UserName = "Ana", Action = AuditAction.CustomerUpdated, EntityName = "Customer" }
        };
        _repo.Setup(r => r.GetPagedAsync(It.IsAny<AuditLogQueryParams>()))
             .ReturnsAsync((logs, 2));

        var result = await _sut.GetAllAsync(new AuditLogQueryParams());

        result.TotalCount.Should().Be(2);
        result.Items.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetAll_MapsFieldsCorrectly()
    {
        var logs = new List<AuditLog>
        {
            new()
            {
                UserId = 5,
                UserName = "João",
                Action = AuditAction.WorkTaskStatusChanged,
                EntityName = "WorkTask",
                EntityId = "10",
                Details = "Pending→InProgress"
            }
        };
        _repo.Setup(r => r.GetPagedAsync(It.IsAny<AuditLogQueryParams>()))
             .ReturnsAsync((logs, 1));

        var result = await _sut.GetAllAsync(new AuditLogQueryParams());
        var item = result.Items.First();

        item.UserId.Should().Be(5);
        item.UserName.Should().Be("João");
        item.Action.Should().Be(AuditAction.WorkTaskStatusChanged);
        item.Details.Should().Be("Pending→InProgress");
    }
}
