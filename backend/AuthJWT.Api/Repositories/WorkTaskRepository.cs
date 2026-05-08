using AuthJWT.Api.Data;
using AuthJWT.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace AuthJWT.Api.Repositories;

public class WorkTaskRepository(AppDbContext db) : IWorkTaskRepository
{
    public async Task<WorkTask> CreateAsync(WorkTask task)
    {
        db.WorkTasks.Add(task);
        await db.SaveChangesAsync();
        return task;
    }

    public Task<WorkTask?> GetByIdAsync(int id) =>
        db.WorkTasks
          .Include(t => t.AssignedUser)
          .FirstOrDefaultAsync(t => t.Id == id);

    public async Task<(IEnumerable<WorkTask> Items, int TotalCount)> GetPagedAsync(
        int page, int pageSize, string? status, int? assignedUserId)
    {
        var query = db.WorkTasks.Include(t => t.AssignedUser).AsQueryable();

        if (!string.IsNullOrWhiteSpace(status))
            query = query.Where(t => t.Status == status);

        if (assignedUserId.HasValue)
            query = query.Where(t => t.AssignedUserId == assignedUserId);

        var total = await query.CountAsync();
        var items = await query
            .OrderBy(t => t.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, total);
    }

    public async Task<WorkTask> UpdateAsync(WorkTask task)
    {
        task.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync();
        return task;
    }
}
