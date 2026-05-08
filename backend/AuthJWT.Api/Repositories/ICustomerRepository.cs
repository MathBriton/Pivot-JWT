using AuthJWT.Api.Models;

namespace AuthJWT.Api.Repositories;

public interface ICustomerRepository
{
    Task<Customer> CreateAsync(Customer customer);
    Task<Customer?> GetByIdAsync(int id);
    Task<(IEnumerable<Customer> Items, int TotalCount)> GetPagedAsync(int page, int pageSize, string? search, string? status);
    Task<Customer> UpdateAsync(Customer customer);
    Task SoftDeleteAsync(Customer customer);
    Task<bool> EmailExistsAsync(string email, int? excludeId = null);
}
