using System.Security.Claims;
using AuthJWT.Api.DTOs;
using AuthJWT.Api.Models;
using AuthJWT.Api.Services;
using AuthJWT.Api.Shared;
using AuthJWT.Api.Validators;

namespace AuthJWT.Api.Endpoints;

public static class WorkTaskEndpoints
{
    public static void MapWorkTaskEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/tasks")
                       .WithTags("Tarefas")
                       .RequireAuthorization();

        group.MapGet("/", async (
            IWorkTaskService service,
            int page = 1,
            int pageSize = 10,
            string? status = null,
            int? assignedUserId = null) =>
        {
            var query = new WorkTaskQueryParams(page, pageSize, status, assignedUserId);
            var result = await service.GetAllAsync(query);
            return Results.Ok(ApiResponse<PagedResult<WorkTaskResponse>>.Ok(result));
        })
        .WithName("ListarTarefas")
        .WithSummary("Listar tarefas com paginação e filtros (Kanban)");

        group.MapGet("/{id:int}", async (int id, IWorkTaskService service) =>
        {
            var task = await service.GetByIdAsync(id);
            return task is null
                ? Results.NotFound(ApiResponse<object>.Fail("Tarefa não encontrada."))
                : Results.Ok(ApiResponse<WorkTaskResponse>.Ok(task));
        })
        .WithName("BuscarTarefa")
        .WithSummary("Buscar tarefa por ID");

        group.MapPost("/", async (CreateWorkTaskRequest request, IWorkTaskService service, IAuditLogService audit, ClaimsPrincipal user) =>
        {
            var validator = new CreateWorkTaskRequestValidator();
            var validation = await validator.ValidateAsync(request);
            if (!validation.IsValid)
            {
                var errors = validation.Errors.Select(e => e.ErrorMessage);
                return Results.BadRequest(ApiResponse<object>.Fail(string.Join(" | ", errors)));
            }

            var result = await service.CreateAsync(request);
            var (userId, userName) = ExtractUser(user);
            await audit.LogAsync(userId, userName, AuditAction.WorkTaskCreated, "WorkTask", result.Id.ToString(), result.Title);
            return Results.Created($"/api/tasks/{result.Id}",
                ApiResponse<WorkTaskResponse>.Ok(result, "Tarefa criada com sucesso."));
        })
        .WithName("CriarTarefa")
        .WithSummary("Criar nova tarefa")
        .RequireAuthorization(policy => policy.RequireRole(UserRoles.Administrator, UserRoles.Manager));

        group.MapPut("/{id:int}", async (int id, UpdateWorkTaskRequest request, IWorkTaskService service, IAuditLogService audit, ClaimsPrincipal user) =>
        {
            if (string.IsNullOrWhiteSpace(request.Title))
                return Results.BadRequest(ApiResponse<object>.Fail("Título é obrigatório."));

            var result = await service.UpdateAsync(id, request);
            if (result is null)
                return Results.NotFound(ApiResponse<object>.Fail("Tarefa não encontrada ou não pode ser editada."));

            var (userId, userName) = ExtractUser(user);
            await audit.LogAsync(userId, userName, AuditAction.WorkTaskUpdated, "WorkTask", id.ToString(), result.Title);
            return Results.Ok(ApiResponse<WorkTaskResponse>.Ok(result, "Tarefa atualizada com sucesso."));
        })
        .WithName("AtualizarTarefa")
        .WithSummary("Atualizar tarefa (não permitido em tarefas concluídas ou canceladas)")
        .RequireAuthorization(policy => policy.RequireRole(UserRoles.Administrator, UserRoles.Manager));

        group.MapPatch("/{id:int}/status", async (int id, ChangeWorkTaskStatusRequest request, IWorkTaskService service, IAuditLogService audit, ClaimsPrincipal user) =>
        {
            var validator = new ChangeWorkTaskStatusRequestValidator();
            var validation = await validator.ValidateAsync(request);
            if (!validation.IsValid)
            {
                var errors = validation.Errors.Select(e => e.ErrorMessage);
                return Results.BadRequest(ApiResponse<object>.Fail(string.Join(" | ", errors)));
            }

            var result = await service.ChangeStatusAsync(id, request);
            if (result is null)
                return Results.BadRequest(ApiResponse<object>.Fail("Transição de status inválida ou tarefa não encontrada."));

            var (userId, userName) = ExtractUser(user);
            await audit.LogAsync(userId, userName, AuditAction.WorkTaskStatusChanged, "WorkTask", id.ToString(), $"→ {request.Status}");
            return Results.Ok(ApiResponse<WorkTaskResponse>.Ok(result, "Status atualizado com sucesso."));
        })
        .WithName("MudarStatusTarefa")
        .WithSummary("Mudar status da tarefa (respeita fluxo Kanban)");
    }

    private static (int? userId, string userName) ExtractUser(ClaimsPrincipal user)
    {
        var idStr = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var userId = idStr is not null ? int.Parse(idStr) : (int?)null;
        var userName = user.FindFirst(ClaimTypes.Email)?.Value ?? "anonymous";
        return (userId, userName);
    }
}
