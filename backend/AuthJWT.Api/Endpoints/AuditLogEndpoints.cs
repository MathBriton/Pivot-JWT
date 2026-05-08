using AuthJWT.Api.DTOs;
using AuthJWT.Api.Models;
using AuthJWT.Api.Services;
using AuthJWT.Api.Shared;

namespace AuthJWT.Api.Endpoints;

public static class AuditLogEndpoints
{
    public static void MapAuditLogEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/audit-logs")
                       .WithTags("Auditoria")
                       .RequireAuthorization(policy => policy.RequireRole(UserRoles.Administrator));

        group.MapGet("/", async (
            IAuditLogService service,
            int page = 1,
            int pageSize = 20,
            string? entityName = null,
            string? action = null,
            int? userId = null) =>
        {
            var query = new AuditLogQueryParams(page, pageSize, entityName, action, userId);
            var result = await service.GetAllAsync(query);
            return Results.Ok(ApiResponse<PagedResult<AuditLogResponse>>.Ok(result));
        })
        .WithName("ListarLogs")
        .WithSummary("Listar logs de auditoria (apenas Administrator)");
    }
}
