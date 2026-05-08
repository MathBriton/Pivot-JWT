using AuthJWT.Api.DTOs;
using AuthJWT.Api.Shared;

namespace AuthJWT.Api.Services;

public interface IWorkTaskService
{
    Task<WorkTaskResponse> CreateAsync(CreateWorkTaskRequest request);
    Task<WorkTaskResponse?> GetByIdAsync(int id);
    Task<PagedResult<WorkTaskResponse>> GetAllAsync(WorkTaskQueryParams query);
    Task<WorkTaskResponse?> UpdateAsync(int id, UpdateWorkTaskRequest request);
    Task<WorkTaskResponse?> ChangeStatusAsync(int id, ChangeWorkTaskStatusRequest request);
}
