namespace AuthJWT.Api.Models;

public class AuditLog
{
    public int Id { get; private set; }
    public int? UserId { get; init; }
    public string UserName { get; init; } = string.Empty;
    public string Action { get; init; } = string.Empty;
    public string EntityName { get; init; } = string.Empty;
    public string? EntityId { get; init; }
    public string? Details { get; init; }
    public DateTime OccurredAt { get; init; } = DateTime.UtcNow;
}
