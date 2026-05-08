using AuthJWT.Api.DTOs;
using AuthJWT.Api.Models;
using AuthJWT.Api.Repositories;
using Microsoft.Extensions.Caching.Memory;

namespace AuthJWT.Api.Services;

public class DashboardService(IDashboardRepository repo, IMemoryCache cache) : IDashboardService
{
    private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(5);
    private const string CacheKey = "dashboard:summary";

    public async Task<DashboardResponse> GetSummaryAsync()
    {
        if (cache.TryGetValue(CacheKey, out DashboardResponse? cached))
            return cached!;

        var activeCustomers  = await repo.CountActiveCustomersAsync();
        var inactiveCustomers = await repo.CountInactiveCustomersAsync();
        var newThisMonth     = await repo.CountNewCustomersThisMonthAsync();
        var taskCounts       = await repo.CountTasksByStatusAsync();
        var totalUsers       = await repo.CountUsersAsync();

        var pending    = taskCounts.GetValueOrDefault(WorkTaskStatus.Pending);
        var inProgress = taskCounts.GetValueOrDefault(WorkTaskStatus.InProgress);
        var review     = taskCounts.GetValueOrDefault(WorkTaskStatus.Review);
        var completed  = taskCounts.GetValueOrDefault(WorkTaskStatus.Completed);
        var cancelled  = taskCounts.GetValueOrDefault(WorkTaskStatus.Cancelled);

        var response = new DashboardResponse(
            Summary: new DashboardSummaryResponse(
                TotalActiveCustomers: activeCustomers,
                TotalOpenTasks: pending + inProgress + review,
                TotalCompletedTasks: completed,
                TotalCancelledTasks: cancelled,
                TotalUsers: totalUsers),
            Tasks: new TaskBreakdownResponse(
                Pending: pending,
                InProgress: inProgress,
                Review: review,
                Completed: completed,
                Cancelled: cancelled,
                Total: pending + inProgress + review + completed + cancelled),
            Customers: new CustomerMetricsResponse(
                TotalCustomers: activeCustomers + inactiveCustomers,
                ActiveCustomers: activeCustomers,
                InactiveCustomers: inactiveCustomers,
                NewThisMonth: newThisMonth));

        cache.Set(CacheKey, response, CacheDuration);
        return response;
    }
}
