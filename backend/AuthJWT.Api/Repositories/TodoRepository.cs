using AuthJWT.Api.Data;
using AuthJWT.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace AuthJWT.Api.Repositories;

public class TodoRepository(AppDbContext db) : ITodoRepository
{
    public async Task<TodoItem?> GetByIdAsync(int id, int userId) =>
        await db.Todos.FirstOrDefaultAsync(t => t.Id == id && t.UserId == userId);

    public async Task<IEnumerable<TodoItem>> GetAllByUserAsync(int userId) =>
        await db.Todos.Where(t => t.UserId == userId).OrderByDescending(t => t.CreatedAt).ToListAsync();

    public async Task<TodoItem> CreateAsync(TodoItem todo)
    {
        db.Todos.Add(todo);
        await db.SaveChangesAsync();
        return todo;
    }

    public async Task<TodoItem?> UpdateAsync(TodoItem todo)
    {
        db.Todos.Update(todo);
        await db.SaveChangesAsync();
        return todo;
    }

    public async Task<bool> DeleteAsync(int id, int userId)
    {
        var todo = await GetByIdAsync(id, userId);
        if (todo is null) return false;
        db.Todos.Remove(todo);
        await db.SaveChangesAsync();
        return true;
    }
}
