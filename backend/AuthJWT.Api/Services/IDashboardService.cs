using AuthJWT.Api.DTOs;

namespace AuthJWT.Api.Services;

public interface IDashboardService
{
    Task<DashboardResponse> GetSummaryAsync();
}
