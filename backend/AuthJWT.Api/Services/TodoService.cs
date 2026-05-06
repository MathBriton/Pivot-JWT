using AuthJWT.Api.DTOs;
using AuthJWT.Api.Models;
using AuthJWT.Api.Repositories;

namespace AuthJWT.Api.Services;

public class TodoService(ITodoRepository todoRepo) : ITodoService
{
    public async Task<IEnumerable<TodoResponse>> GetAllAsync(int userId)
    {
        var todos = await todoRepo.GetAllByUserAsync(userId);
        return todos.Select(ToResponse);
    }

    public async Task<TodoResponse?> GetByIdAsync(int id, int userId)
    {
        var todo = await todoRepo.GetByIdAsync(id, userId);
        return todo is null ? null : ToResponse(todo);
    }

    public async Task<TodoResponse> CreateAsync(CreateTodoRequest request, int userId)
    {
        var todo = new TodoItem
        {
            Title = request.Title,
            Description = request.Description,
            UserId = userId
        };
        var created = await todoRepo.CreateAsync(todo);
        return ToResponse(created);
    }

    public async Task<TodoResponse?> UpdateAsync(int id, UpdateTodoRequest request, int userId)
    {
        var todo = await todoRepo.GetByIdAsync(id, userId);
        if (todo is null) return null;

        todo.Title = request.Title;
        todo.Description = request.Description;
        todo.IsCompleted = request.IsCompleted;
        todo.CompletedAt = request.IsCompleted ? DateTime.UtcNow : null;

        var updated = await todoRepo.UpdateAsync(todo);
        return updated is null ? null : ToResponse(updated);
    }

    public Task<bool> DeleteAsync(int id, int userId) =>
        todoRepo.DeleteAsync(id, userId);

    private static TodoResponse ToResponse(TodoItem t) =>
        new(t.Id, t.Title, t.Description, t.IsCompleted, t.CreatedAt, t.CompletedAt, t.UserId);
}
