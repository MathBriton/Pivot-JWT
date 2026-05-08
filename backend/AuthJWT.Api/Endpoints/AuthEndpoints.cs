using System.Security.Claims;
using AuthJWT.Api.DTOs;
using AuthJWT.Api.Models;
using AuthJWT.Api.Services;
using AuthJWT.Api.Shared;
using AuthJWT.Api.Validators;

namespace AuthJWT.Api.Endpoints;

public static class AuthEndpoints
{
    public static void MapAuthEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/auth").WithTags("Auth");

        group.MapPost("/register", async (RegisterRequest request, IAuthService authService, IAuditLogService audit) =>
        {
            var validator = new RegisterRequestValidator();
            var validation = await validator.ValidateAsync(request);
            if (!validation.IsValid)
            {
                var errors = validation.Errors.Select(e => e.ErrorMessage);
                return Results.BadRequest(ApiResponse<object>.Fail(string.Join(" | ", errors)));
            }

            var result = await authService.RegisterAsync(request);
            if (result is null)
                return Results.Conflict(ApiResponse<object>.Fail("E-mail já cadastrado."));

            await audit.LogAsync(null, result.Email, AuditAction.AuthRegister, "Auth", details: result.Name);
            return Results.Ok(ApiResponse<AuthResponse>.Ok(result, "Usuário registrado com sucesso."));
        })
        .WithName("Register")
        .WithSummary("Registrar novo usuário")
        .AllowAnonymous();

        group.MapPost("/login", async (LoginRequest request, IAuthService authService, IAuditLogService audit) =>
        {
            var validator = new LoginRequestValidator();
            var validation = await validator.ValidateAsync(request);
            if (!validation.IsValid)
            {
                var errors = validation.Errors.Select(e => e.ErrorMessage);
                return Results.BadRequest(ApiResponse<object>.Fail(string.Join(" | ", errors)));
            }

            var result = await authService.LoginAsync(request);
            if (result is null)
            {
                await audit.LogAsync(null, request.Email, AuditAction.AuthLoginFailed, "Auth");
                return Results.Unauthorized();
            }

            await audit.LogAsync(null, result.Email, AuditAction.AuthLogin, "Auth");
            return Results.Ok(ApiResponse<AuthResponse>.Ok(result, "Login realizado com sucesso."));
        })
        .WithName("Login")
        .WithSummary("Login e obtenção do token JWT")
        .AllowAnonymous();

        group.MapPost("/refresh", async (RefreshRequest request, IAuthService authService, IAuditLogService audit) =>
        {
            if (string.IsNullOrWhiteSpace(request.Token))
                return Results.BadRequest(ApiResponse<object>.Fail("Token de refresh é obrigatório."));

            var result = await authService.RefreshAsync(request.Token);
            if (result is null)
                return Results.Unauthorized();

            await audit.LogAsync(null, result.Email, AuditAction.AuthRefreshToken, "Auth");
            return Results.Ok(ApiResponse<AuthResponse>.Ok(result, "Token renovado com sucesso."));
        })
        .WithName("Refresh")
        .WithSummary("Renovar access token via refresh token")
        .AllowAnonymous();

        group.MapPost("/logout", async (RefreshRequest request, IAuthService authService, IAuditLogService audit, ClaimsPrincipal user) =>
        {
            var revoked = await authService.LogoutAsync(request.Token);
            if (!revoked)
                return Results.BadRequest(ApiResponse<object>.Fail("Token inválido."));

            var (userId, userName) = ExtractUser(user);
            await audit.LogAsync(userId, userName, AuditAction.AuthLogout, "Auth");
            return Results.Ok(ApiResponse<object>.Ok(null!, "Logout realizado com sucesso."));
        })
        .WithName("Logout")
        .WithSummary("Revogar refresh token (logout)")
        .RequireAuthorization();
    }

    private static (int? userId, string userName) ExtractUser(ClaimsPrincipal user)
    {
        var idStr = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var userId = idStr is not null ? int.Parse(idStr) : (int?)null;
        var userName = user.FindFirst(ClaimTypes.Email)?.Value ?? "anonymous";
        return (userId, userName);
    }
}
