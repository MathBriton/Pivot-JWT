namespace AuthJWT.Api.Repositories;

public interface IDashboardRepository
{
    Task<int> CountActiveCustomersAsync();
    Task<int> CountInactiveCustomersAsync();
    Task<int> CountNewCustomersThisMonthAsync();
    Task<Dictionary<string, int>> CountTasksByStatusAsync();
    Task<int> CountUsersAsync();
}
