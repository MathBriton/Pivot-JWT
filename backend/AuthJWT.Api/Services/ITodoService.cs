using AuthJWT.Api.DTOs;

namespace AuthJWT.Api.Services;

public interface ITodoService
{
    Task<IEnumerable<TodoResponse>> GetAllAsync(int userId);
    Task<TodoResponse?> GetByIdAsync(int id, int userId);
    Task<TodoResponse> CreateAsync(CreateTodoRequest request, int userId);
    Task<TodoResponse?> UpdateAsync(int id, UpdateTodoRequest request, int userId);
    Task<bool> DeleteAsync(int id, int userId);
}
