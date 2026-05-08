using System.Security.Claims;
using AuthJWT.Api.DTOs;
using AuthJWT.Api.Models;
using AuthJWT.Api.Services;
using AuthJWT.Api.Shared;
using AuthJWT.Api.Validators;

namespace AuthJWT.Api.Endpoints;

public static class CustomerEndpoints
{
    public static void MapCustomerEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/customers")
                       .WithTags("Clientes")
                       .RequireAuthorization();

        group.MapGet("/", async (
            ICustomerService service,
            int page = 1,
            int pageSize = 10,
            string? search = null,
            string? status = null) =>
        {
            var query = new CustomerQueryParams(page, pageSize, search, status);
            var result = await service.GetAllAsync(query);
            return Results.Ok(ApiResponse<PagedResult<CustomerResponse>>.Ok(result));
        })
        .WithName("ListarClientes")
        .WithSummary("Listar clientes com paginação e filtros");

        group.MapGet("/{id:int}", async (int id, ICustomerService service) =>
        {
            var customer = await service.GetByIdAsync(id);
            return customer is null
                ? Results.NotFound(ApiResponse<object>.Fail("Cliente não encontrado."))
                : Results.Ok(ApiResponse<CustomerResponse>.Ok(customer));
        })
        .WithName("BuscarCliente")
        .WithSummary("Buscar cliente por ID");

        group.MapPost("/", async (CreateCustomerRequest request, ICustomerService service, IAuditLogService audit, ClaimsPrincipal user) =>
        {
            var validator = new CreateCustomerRequestValidator();
            var validation = await validator.ValidateAsync(request);
            if (!validation.IsValid)
            {
                var errors = validation.Errors.Select(e => e.ErrorMessage);
                return Results.BadRequest(ApiResponse<object>.Fail(string.Join(" | ", errors)));
            }

            var result = await service.CreateAsync(request);
            if (result is null)
                return Results.Conflict(ApiResponse<object>.Fail("E-mail já cadastrado."));

            var (userId, userName) = ExtractUser(user);
            await audit.LogAsync(userId, userName, AuditAction.CustomerCreated, "Customer", result.Id.ToString(), result.Name);
            return Results.Created($"/api/customers/{result.Id}", ApiResponse<CustomerResponse>.Ok(result, "Cliente criado com sucesso."));
        })
        .WithName("CriarCliente")
        .WithSummary("Criar novo cliente")
        .RequireAuthorization(policy => policy.RequireRole(UserRoles.Administrator, UserRoles.Manager));

        group.MapPut("/{id:int}", async (int id, UpdateCustomerRequest request, ICustomerService service, IAuditLogService audit, ClaimsPrincipal user) =>
        {
            var validator = new UpdateCustomerRequestValidator();
            var validation = await validator.ValidateAsync(request);
            if (!validation.IsValid)
            {
                var errors = validation.Errors.Select(e => e.ErrorMessage);
                return Results.BadRequest(ApiResponse<object>.Fail(string.Join(" | ", errors)));
            }

            var result = await service.UpdateAsync(id, request);
            if (result is null)
                return Results.NotFound(ApiResponse<object>.Fail("Cliente não encontrado ou e-mail já cadastrado."));

            var (userId, userName) = ExtractUser(user);
            await audit.LogAsync(userId, userName, AuditAction.CustomerUpdated, "Customer", id.ToString(), result.Name);
            return Results.Ok(ApiResponse<CustomerResponse>.Ok(result, "Cliente atualizado com sucesso."));
        })
        .WithName("AtualizarCliente")
        .WithSummary("Atualizar dados do cliente")
        .RequireAuthorization(policy => policy.RequireRole(UserRoles.Administrator, UserRoles.Manager));

        group.MapDelete("/{id:int}", async (int id, ICustomerService service, IAuditLogService audit, ClaimsPrincipal user) =>
        {
            var deleted = await service.DeleteAsync(id);
            if (!deleted)
                return Results.NotFound(ApiResponse<object>.Fail("Cliente não encontrado."));

            var (userId, userName) = ExtractUser(user);
            await audit.LogAsync(userId, userName, AuditAction.CustomerDeleted, "Customer", id.ToString());
            return Results.Ok(ApiResponse<object>.Ok(null!, "Cliente removido com sucesso."));
        })
        .WithName("RemoverCliente")
        .WithSummary("Remover cliente (soft delete)")
        .RequireAuthorization(policy => policy.RequireRole(UserRoles.Administrator, UserRoles.Manager));
    }

    private static (int? userId, string userName) ExtractUser(ClaimsPrincipal user)
    {
        var idStr = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var userId = idStr is not null ? int.Parse(idStr) : (int?)null;
        var userName = user.FindFirst(ClaimTypes.Email)?.Value ?? "anonymous";
        return (userId, userName);
    }
}
