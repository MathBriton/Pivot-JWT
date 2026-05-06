using AuthJWT.Api.DTOs;
using AuthJWT.Api.Services;

namespace AuthJWT.Api.Endpoints;

public static class AuthEndpoints
{
    public static void MapAuthEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/auth").WithTags("Auth");

        group.MapPost("/register", async (RegisterRequest request, IAuthService authService) =>
        {
            if (string.IsNullOrWhiteSpace(request.Name) ||
                string.IsNullOrWhiteSpace(request.Email) ||
                string.IsNullOrWhiteSpace(request.Password))
                return Results.BadRequest(new { message = "Name, email and password are required." });

            if (request.Password.Length < 6)
                return Results.BadRequest(new { message = "Password must be at least 6 characters." });

            var result = await authService.RegisterAsync(request);
            return result is null
                ? Results.Conflict(new { message = "Email already registered." })
                : Results.Ok(result);
        })
        .WithName("Register")
        .WithSummary("Register a new user")
        .AllowAnonymous();

        group.MapPost("/login", async (LoginRequest request, IAuthService authService) =>
        {
            var result = await authService.LoginAsync(request);
            return result is null
                ? Results.Unauthorized()
                : Results.Ok(result);
        })
        .WithName("Login")
        .WithSummary("Login and get JWT token")
        .AllowAnonymous();
    }
}
