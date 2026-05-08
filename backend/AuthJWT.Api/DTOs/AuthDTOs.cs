namespace AuthJWT.Api.DTOs;

public record RegisterRequest(string Name, string Email, string Password);

public record LoginRequest(string Email, string Password);

public record AuthResponse(string AccessToken, string RefreshToken, string Name, string Email, string Role);

public record RefreshRequest(string Token);

public record UserResponse(int Id, string Name, string Email, string Role, DateTime CreatedAt);
