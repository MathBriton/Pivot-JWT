using AuthJWT.Api.DTOs;
using AuthJWT.Api.Models;

namespace AuthJWT.Api.Repositories;

public interface IAuditLogRepository
{
    Task AddAsync(AuditLog log);
    Task<(IEnumerable<AuditLog> Items, int TotalCount)> GetPagedAsync(AuditLogQueryParams query);
}
