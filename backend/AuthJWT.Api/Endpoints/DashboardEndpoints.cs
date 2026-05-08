using AuthJWT.Api.DTOs;
using AuthJWT.Api.Services;
using AuthJWT.Api.Shared;

namespace AuthJWT.Api.Endpoints;

public static class DashboardEndpoints
{
    public static void MapDashboardEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/dashboard")
                       .WithTags("Dashboard")
                       .RequireAuthorization();

        group.MapGet("/", async (IDashboardService service) =>
        {
            var result = await service.GetSummaryAsync();
            return Results.Ok(ApiResponse<DashboardResponse>.Ok(result));
        })
        .WithName("ObterDashboard")
        .WithSummary("KPIs, breakdown de tarefas e métricas de clientes (cache 5min)");
    }
}
