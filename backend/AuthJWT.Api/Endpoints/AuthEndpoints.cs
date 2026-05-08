using AuthJWT.Api.DTOs;
using AuthJWT.Api.Services;
using AuthJWT.Api.Shared;
using AuthJWT.Api.Validators;

namespace AuthJWT.Api.Endpoints;

public static class AuthEndpoints
{
    public static void MapAuthEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/auth").WithTags("Auth");

        group.MapPost("/register", async (RegisterRequest request, IAuthService authService) =>
        {
            var validator = new RegisterRequestValidator();
            var validation = await validator.ValidateAsync(request);
            if (!validation.IsValid)
            {
                var errors = validation.Errors.Select(e => e.ErrorMessage);
                return Results.BadRequest(ApiResponse<object>.Fail(string.Join(" | ", errors)));
            }

            var result = await authService.RegisterAsync(request);
            return result is null
                ? Results.Conflict(ApiResponse<object>.Fail("E-mail já cadastrado."))
                : Results.Ok(ApiResponse<AuthResponse>.Ok(result, "Usuário registrado com sucesso."));
        })
        .WithName("Register")
        .WithSummary("Registrar novo usuário")
        .AllowAnonymous();

        group.MapPost("/login", async (LoginRequest request, IAuthService authService) =>
        {
            var validator = new LoginRequestValidator();
            var validation = await validator.ValidateAsync(request);
            if (!validation.IsValid)
            {
                var errors = validation.Errors.Select(e => e.ErrorMessage);
                return Results.BadRequest(ApiResponse<object>.Fail(string.Join(" | ", errors)));
            }

            var result = await authService.LoginAsync(request);
            return result is null
                ? Results.Unauthorized()
                : Results.Ok(ApiResponse<AuthResponse>.Ok(result, "Login realizado com sucesso."));
        })
        .WithName("Login")
        .WithSummary("Login e obtenção do token JWT")
        .AllowAnonymous();

        group.MapPost("/refresh", async (RefreshRequest request, IAuthService authService) =>
        {
            if (string.IsNullOrWhiteSpace(request.Token))
                return Results.BadRequest(ApiResponse<object>.Fail("Token de refresh é obrigatório."));

            var result = await authService.RefreshAsync(request.Token);
            return result is null
                ? Results.Unauthorized()
                : Results.Ok(ApiResponse<AuthResponse>.Ok(result, "Token renovado com sucesso."));
        })
        .WithName("Refresh")
        .WithSummary("Renovar access token via refresh token")
        .AllowAnonymous();

        group.MapPost("/logout", async (RefreshRequest request, IAuthService authService) =>
        {
            var revoked = await authService.LogoutAsync(request.Token);
            return revoked
                ? Results.Ok(ApiResponse<object>.Ok(null!, "Logout realizado com sucesso."))
                : Results.BadRequest(ApiResponse<object>.Fail("Token inválido."));
        })
        .WithName("Logout")
        .WithSummary("Revogar refresh token (logout)")
        .RequireAuthorization();
    }
}
