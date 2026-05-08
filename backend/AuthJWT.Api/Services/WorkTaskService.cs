using AuthJWT.Api.DTOs;
using AuthJWT.Api.Models;
using AuthJWT.Api.Repositories;
using AuthJWT.Api.Shared;

namespace AuthJWT.Api.Services;

public class WorkTaskService(IWorkTaskRepository repo) : IWorkTaskService
{
    public async Task<WorkTaskResponse> CreateAsync(CreateWorkTaskRequest request)
    {
        var task = new WorkTask
        {
            Title = request.Title,
            Description = request.Description ?? string.Empty,
            AssignedUserId = request.AssignedUserId,
            Deadline = request.Deadline
        };

        await repo.CreateAsync(task);
        return ToResponse(task);
    }

    public async Task<WorkTaskResponse?> GetByIdAsync(int id)
    {
        var task = await repo.GetByIdAsync(id);
        return task is null ? null : ToResponse(task);
    }

    public async Task<PagedResult<WorkTaskResponse>> GetAllAsync(WorkTaskQueryParams query)
    {
        var page = Math.Max(1, query.Page);
        var pageSize = Math.Clamp(query.PageSize, 1, 100);

        var (items, total) = await repo.GetPagedAsync(page, pageSize, query.Status, query.AssignedUserId);
        return PagedResult<WorkTaskResponse>.From(items.Select(ToResponse), total, page, pageSize);
    }

    public async Task<WorkTaskResponse?> UpdateAsync(int id, UpdateWorkTaskRequest request)
    {
        var task = await repo.GetByIdAsync(id);
        if (task is null || task.IsClosed)
            return null;

        task.Title = request.Title;
        task.Description = request.Description ?? string.Empty;
        task.AssignedUserId = request.AssignedUserId;
        task.Deadline = request.Deadline;

        await repo.UpdateAsync(task);
        return ToResponse(task);
    }

    public async Task<WorkTaskResponse?> ChangeStatusAsync(int id, ChangeWorkTaskStatusRequest request)
    {
        var task = await repo.GetByIdAsync(id);
        if (task is null || !task.CanTransitionTo(request.Status))
            return null;

        task.Status = request.Status;
        await repo.UpdateAsync(task);
        return ToResponse(task);
    }

    private static WorkTaskResponse ToResponse(WorkTask t) =>
        new(t.Id, t.Title, t.Description, t.Status,
            t.AssignedUserId, t.AssignedUser?.Name,
            t.Deadline, t.CreatedAt, t.UpdatedAt);
}
