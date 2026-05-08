using AuthJWT.Api.DTOs;
using AuthJWT.Api.Models;
using AuthJWT.Api.Repositories;
using AuthJWT.Api.Shared;

namespace AuthJWT.Api.Services;

public class AuditLogService(IAuditLogRepository repo) : IAuditLogService
{
    public async Task LogAsync(int? userId, string userName, string action, string entityName,
                               string? entityId = null, string? details = null)
    {
        var log = new AuditLog
        {
            UserId = userId,
            UserName = userName,
            Action = action,
            EntityName = entityName,
            EntityId = entityId,
            Details = details
        };
        await repo.AddAsync(log);
    }

    public async Task<PagedResult<AuditLogResponse>> GetAllAsync(AuditLogQueryParams query)
    {
        var page = Math.Max(1, query.Page);
        var pageSize = Math.Clamp(query.PageSize, 1, 100);
        var normalized = query with { Page = page, PageSize = pageSize };

        var (items, total) = await repo.GetPagedAsync(normalized);
        return PagedResult<AuditLogResponse>.From(items.Select(ToResponse), total, page, pageSize);
    }

    private static AuditLogResponse ToResponse(AuditLog l) =>
        new(l.Id, l.UserId, l.UserName, l.Action, l.EntityName, l.EntityId, l.Details, l.OccurredAt);
}
