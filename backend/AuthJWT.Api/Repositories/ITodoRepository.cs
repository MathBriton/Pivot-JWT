using AuthJWT.Api.Models;

namespace AuthJWT.Api.Repositories;

public interface ITodoRepository
{
    Task<TodoItem?> GetByIdAsync(int id, int userId);
    Task<IEnumerable<TodoItem>> GetAllByUserAsync(int userId);
    Task<TodoItem> CreateAsync(TodoItem todo);
    Task<TodoItem?> UpdateAsync(TodoItem todo);
    Task<bool> DeleteAsync(int id, int userId);
}
