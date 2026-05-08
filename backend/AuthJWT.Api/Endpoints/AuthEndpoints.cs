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
    }
}
