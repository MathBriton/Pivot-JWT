namespace AuthJWT.Api.DTOs;

public record DashboardSummaryResponse(
    int TotalActiveCustomers,
    int TotalOpenTasks,
    int TotalCompletedTasks,
    int TotalCancelledTasks,
    int TotalUsers);

public record TaskBreakdownResponse(
    int Pending,
    int InProgress,
    int Review,
    int Completed,
    int Cancelled,
    int Total);

public record CustomerMetricsResponse(
    int TotalCustomers,
    int ActiveCustomers,
    int InactiveCustomers,
    int NewThisMonth);

public record DashboardResponse(
    DashboardSummaryResponse Summary,
    TaskBreakdownResponse Tasks,
    CustomerMetricsResponse Customers);
