using AuthJWT.Api.DTOs;
using AuthJWT.Api.Models;
using AuthJWT.Api.Repositories;
using AuthJWT.Api.Services;
using FluentAssertions;
using Microsoft.Extensions.Caching.Memory;
using Moq;

namespace AuthJWT.Api.Tests.Services;

public class DashboardServiceTests
{
    private readonly Mock<IDashboardRepository> _repo;
    private readonly IMemoryCache _cache;
    private readonly DashboardService _sut;

    public DashboardServiceTests()
    {
        _repo = new Mock<IDashboardRepository>();
        _cache = new MemoryCache(new MemoryCacheOptions());
        _sut = new DashboardService(_repo.Object, _cache);
    }

    private static Dictionary<string, int> TaskCounts(int pending = 0, int inProgress = 0,
        int review = 0, int completed = 0, int cancelled = 0) =>
        new()
        {
            [WorkTaskStatus.Pending]    = pending,
            [WorkTaskStatus.InProgress] = inProgress,
            [WorkTaskStatus.Review]     = review,
            [WorkTaskStatus.Completed]  = completed,
            [WorkTaskStatus.Cancelled]  = cancelled
        };

    private void SetupRepo(int active = 0, int inactive = 0, int newThisMonth = 0,
        int users = 0, Dictionary<string, int>? tasks = null)
    {
        _repo.Setup(r => r.CountActiveCustomersAsync()).ReturnsAsync(active);
        _repo.Setup(r => r.CountInactiveCustomersAsync()).ReturnsAsync(inactive);
        _repo.Setup(r => r.CountNewCustomersThisMonthAsync()).ReturnsAsync(newThisMonth);
        _repo.Setup(r => r.CountUsersAsync()).ReturnsAsync(users);
        _repo.Setup(r => r.CountTasksByStatusAsync()).ReturnsAsync(tasks ?? TaskCounts());
    }

    // --- Summary ---

    [Fact]
    public async Task GetSummary_ReturnsAggregatedKPIs()
    {
        SetupRepo(active: 10, inactive: 3, newThisMonth: 2, users: 5,
            tasks: TaskCounts(pending: 4, inProgress: 2, review: 1, completed: 3, cancelled: 1));

        var result = await _sut.GetSummaryAsync();

        result.Summary.TotalActiveCustomers.Should().Be(10);
        result.Summary.TotalUsers.Should().Be(5);
        result.Summary.TotalOpenTasks.Should().Be(7);      // Pending + InProgress + Review
        result.Summary.TotalCompletedTasks.Should().Be(3);
        result.Summary.TotalCancelledTasks.Should().Be(1);
    }

    // --- Task Breakdown ---

    [Fact]
    public async Task GetSummary_TaskBreakdown_ShowsAllStatuses()
    {
        SetupRepo(tasks: TaskCounts(pending: 4, inProgress: 2, review: 1, completed: 3, cancelled: 1));

        var result = await _sut.GetSummaryAsync();

        result.Tasks.Pending.Should().Be(4);
        result.Tasks.InProgress.Should().Be(2);
        result.Tasks.Review.Should().Be(1);
        result.Tasks.Completed.Should().Be(3);
        result.Tasks.Cancelled.Should().Be(1);
        result.Tasks.Total.Should().Be(11);
    }

    [Fact]
    public async Task GetSummary_TaskBreakdown_WhenNoTasks_AllZero()
    {
        SetupRepo();

        var result = await _sut.GetSummaryAsync();

        result.Tasks.Total.Should().Be(0);
        result.Tasks.Pending.Should().Be(0);
    }

    // --- Customer Metrics ---

    [Fact]
    public async Task GetSummary_CustomerMetrics_AreCorrect()
    {
        SetupRepo(active: 10, inactive: 3, newThisMonth: 2);

        var result = await _sut.GetSummaryAsync();

        result.Customers.TotalCustomers.Should().Be(13);
        result.Customers.ActiveCustomers.Should().Be(10);
        result.Customers.InactiveCustomers.Should().Be(3);
        result.Customers.NewThisMonth.Should().Be(2);
    }

    // --- Cache ---

    [Fact]
    public async Task GetSummary_OnSecondCall_UsesCachedResult()
    {
        SetupRepo(active: 5, users: 2);

        await _sut.GetSummaryAsync();
        await _sut.GetSummaryAsync();

        _repo.Verify(r => r.CountActiveCustomersAsync(), Times.Once);
    }

    [Fact]
    public async Task GetSummary_ReturnsNonNullResult()
    {
        SetupRepo();

        var result = await _sut.GetSummaryAsync();

        result.Should().NotBeNull();
        result.Summary.Should().NotBeNull();
        result.Tasks.Should().NotBeNull();
        result.Customers.Should().NotBeNull();
    }
}
