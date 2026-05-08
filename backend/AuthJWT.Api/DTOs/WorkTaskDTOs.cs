namespace AuthJWT.Api.DTOs;

public record CreateWorkTaskRequest(
    string Title,
    string? Description,
    int? AssignedUserId,
    DateTime? Deadline);

public record UpdateWorkTaskRequest(
    string Title,
    string? Description,
    int? AssignedUserId,
    DateTime? Deadline);

public record ChangeWorkTaskStatusRequest(string Status);

public record WorkTaskResponse(
    int Id,
    string Title,
    string Description,
    string Status,
    int? AssignedUserId,
    string? AssignedUserName,
    DateTime? Deadline,
    DateTime CreatedAt,
    DateTime UpdatedAt);

public record WorkTaskQueryParams(
    int Page = 1,
    int PageSize = 10,
    string? Status = null,
    int? AssignedUserId = null);
