namespace AuthJWT.Api.DTOs;

public record CreateCustomerRequest(
    string Name,
    string Email,
    string? Phone,
    string? Company,
    string? Notes);

public record UpdateCustomerRequest(
    string Name,
    string Email,
    string? Phone,
    string? Company,
    string Status,
    string? Notes);

public record CustomerResponse(
    int Id,
    string Name,
    string Email,
    string Phone,
    string Company,
    string Status,
    string Notes,
    DateTime CreatedAt,
    DateTime UpdatedAt);

public record CustomerQueryParams(
    int Page = 1,
    int PageSize = 10,
    string? Search = null,
    string? Status = null);
