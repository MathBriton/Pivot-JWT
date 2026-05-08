using AuthJWT.Api.DTOs;

namespace AuthJWT.Api.Services;

public interface IAuthService
{
    Task<AuthResponse?> RegisterAsync(RegisterRequest request);
    Task<AuthResponse?> LoginAsync(LoginRequest request);
    Task<AuthResponse?> RefreshAsync(string refreshToken);
    Task<bool> LogoutAsync(string refreshToken);
}
