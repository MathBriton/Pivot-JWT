namespace AuthJWT.Api.DTOs;

public record AuditLogResponse(
    int Id,
    int? UserId,
    string UserName,
    string Action,
    string EntityName,
    string? EntityId,
    string? Details,
    DateTime OccurredAt);

public record AuditLogQueryParams(
    int Page = 1,
    int PageSize = 20,
    string? EntityName = null,
    string? Action = null,
    int? UserId = null);
