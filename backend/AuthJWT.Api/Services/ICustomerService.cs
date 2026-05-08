using AuthJWT.Api.DTOs;
using AuthJWT.Api.Shared;

namespace AuthJWT.Api.Services;

public interface ICustomerService
{
    Task<CustomerResponse?> CreateAsync(CreateCustomerRequest request);
    Task<CustomerResponse?> GetByIdAsync(int id);
    Task<PagedResult<CustomerResponse>> GetAllAsync(CustomerQueryParams query);
    Task<CustomerResponse?> UpdateAsync(int id, UpdateCustomerRequest request);
    Task<bool> DeleteAsync(int id);
}
