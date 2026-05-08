using AuthJWT.Api.Models;

namespace AuthJWT.Api.Repositories;

public interface IWorkTaskRepository
{
    Task<WorkTask> CreateAsync(WorkTask task);
    Task<WorkTask?> GetByIdAsync(int id);
    Task<(IEnumerable<WorkTask> Items, int TotalCount)> GetPagedAsync(
        int page, int pageSize, string? status, int? assignedUserId);
    Task<WorkTask> UpdateAsync(WorkTask task);
}
