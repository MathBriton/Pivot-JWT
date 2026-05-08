using AuthJWT.Api.Data;
using AuthJWT.Api.DTOs;
using AuthJWT.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace AuthJWT.Api.Repositories;

public class AuditLogRepository(AppDbContext db) : IAuditLogRepository
{
    public async Task AddAsync(AuditLog log)
    {
        db.AuditLogs.Add(log);
        await db.SaveChangesAsync();
    }

    public async Task<(IEnumerable<AuditLog> Items, int TotalCount)> GetPagedAsync(AuditLogQueryParams query)
    {
        var q = db.AuditLogs.AsQueryable();

        if (!string.IsNullOrWhiteSpace(query.EntityName))
            q = q.Where(l => l.EntityName == query.EntityName);

        if (!string.IsNullOrWhiteSpace(query.Action))
            q = q.Where(l => l.Action == query.Action);

        if (query.UserId.HasValue)
            q = q.Where(l => l.UserId == query.UserId);

        var total = await q.CountAsync();
        var items = await q
            .OrderByDescending(l => l.OccurredAt)
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToListAsync();

        return (items, total);
    }
}
