using AuthJWT.Api.DTOs;
using AuthJWT.Api.Shared;

namespace AuthJWT.Api.Services;

public interface IAuditLogService
{
    Task LogAsync(int? userId, string userName, string action, string entityName,
                  string? entityId = null, string? details = null);

    Task<PagedResult<AuditLogResponse>> GetAllAsync(AuditLogQueryParams query);
}
