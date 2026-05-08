using AuthJWT.Api.Data;
using AuthJWT.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace AuthJWT.Api.Repositories;

public class DashboardRepository(AppDbContext db) : IDashboardRepository
{
    public Task<int> CountActiveCustomersAsync() =>
        db.Customers.CountAsync(c => !c.IsDeleted && c.Status == CustomerStatus.Active);

    public Task<int> CountInactiveCustomersAsync() =>
        db.Customers.CountAsync(c => !c.IsDeleted && c.Status == CustomerStatus.Inactive);

    public Task<int> CountNewCustomersThisMonthAsync()
    {
        var startOfMonth = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1, 0, 0, 0, DateTimeKind.Utc);
        return db.Customers.CountAsync(c => !c.IsDeleted && c.CreatedAt >= startOfMonth);
    }

    public async Task<Dictionary<string, int>> CountTasksByStatusAsync()
    {
        var counts = await db.WorkTasks
            .GroupBy(t => t.Status)
            .Select(g => new { Status = g.Key, Count = g.Count() })
            .ToListAsync();

        var result = WorkTaskStatus.All.ToDictionary(s => s, _ => 0);
        foreach (var c in counts)
            result[c.Status] = c.Count;

        return result;
    }

    public Task<int> CountUsersAsync() => db.Users.CountAsync();
}
