using System.Security.Claims;
using AuthJWT.Api.DTOs;
using AuthJWT.Api.Services;

namespace AuthJWT.Api.Endpoints;

public static class TodoEndpoints
{
    public static void MapTodoEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/todos").WithTags("Todos").RequireAuthorization();

        group.MapGet("/", async (ClaimsPrincipal user, ITodoService todoService) =>
        {
            var userId = GetUserId(user);
            var todos = await todoService.GetAllAsync(userId);
            return Results.Ok(todos);
        })
        .WithName("GetTodos")
        .WithSummary("Get all todos for the authenticated user");

        group.MapGet("/{id:int}", async (int id, ClaimsPrincipal user, ITodoService todoService) =>
        {
            var userId = GetUserId(user);
            var todo = await todoService.GetByIdAsync(id, userId);
            return todo is null ? Results.NotFound() : Results.Ok(todo);
        })
        .WithName("GetTodoById")
        .WithSummary("Get a specific todo");

        group.MapPost("/", async (CreateTodoRequest request, ClaimsPrincipal user, ITodoService todoService) =>
        {
            if (string.IsNullOrWhiteSpace(request.Title))
                return Results.BadRequest(new { message = "Title is required." });

            var userId = GetUserId(user);
            var todo = await todoService.CreateAsync(request, userId);
            return Results.Created($"/api/todos/{todo.Id}", todo);
        })
        .WithName("CreateTodo")
        .WithSummary("Create a new todo");

        group.MapPut("/{id:int}", async (int id, UpdateTodoRequest request, ClaimsPrincipal user, ITodoService todoService) =>
        {
            if (string.IsNullOrWhiteSpace(request.Title))
                return Results.BadRequest(new { message = "Title is required." });

            var userId = GetUserId(user);
            var todo = await todoService.UpdateAsync(id, request, userId);
            return todo is null ? Results.NotFound() : Results.Ok(todo);
        })
        .WithName("UpdateTodo")
        .WithSummary("Update a todo");

        group.MapDelete("/{id:int}", async (int id, ClaimsPrincipal user, ITodoService todoService) =>
        {
            var userId = GetUserId(user);
            var deleted = await todoService.DeleteAsync(id, userId);
            return deleted ? Results.NoContent() : Results.NotFound();
        })
        .WithName("DeleteTodo")
        .WithSummary("Delete a todo");
    }

    private static int GetUserId(ClaimsPrincipal user) =>
        int.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)!);
}
