using AuthJWT.Api.DTOs;
using AuthJWT.Api.Models;
using AuthJWT.Api.Repositories;
using AuthJWT.Api.Shared;

namespace AuthJWT.Api.Services;

public class CustomerService(ICustomerRepository repo) : ICustomerService
{
    public async Task<CustomerResponse?> CreateAsync(CreateCustomerRequest request)
    {
        if (await repo.EmailExistsAsync(request.Email))
            return null;

        var customer = new Customer
        {
            Name = request.Name,
            Email = request.Email.ToLower(),
            Phone = request.Phone ?? string.Empty,
            Company = request.Company ?? string.Empty,
            Notes = request.Notes ?? string.Empty
        };

        await repo.CreateAsync(customer);
        return ToResponse(customer);
    }

    public async Task<CustomerResponse?> GetByIdAsync(int id)
    {
        var customer = await repo.GetByIdAsync(id);
        return customer is null ? null : ToResponse(customer);
    }

    public async Task<PagedResult<CustomerResponse>> GetAllAsync(CustomerQueryParams query)
    {
        var page = Math.Max(1, query.Page);
        var pageSize = Math.Clamp(query.PageSize, 1, 100);

        var (items, total) = await repo.GetPagedAsync(page, pageSize, query.Search, query.Status);
        return PagedResult<CustomerResponse>.From(items.Select(ToResponse), total, page, pageSize);
    }

    public async Task<CustomerResponse?> UpdateAsync(int id, UpdateCustomerRequest request)
    {
        var customer = await repo.GetByIdAsync(id);
        if (customer is null)
            return null;

        if (await repo.EmailExistsAsync(request.Email, excludeId: id))
            return null;

        customer.Name = request.Name;
        customer.Email = request.Email.ToLower();
        customer.Phone = request.Phone ?? string.Empty;
        customer.Company = request.Company ?? string.Empty;
        customer.Status = request.Status;
        customer.Notes = request.Notes ?? string.Empty;

        await repo.UpdateAsync(customer);
        return ToResponse(customer);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var customer = await repo.GetByIdAsync(id);
        if (customer is null)
            return false;

        await repo.SoftDeleteAsync(customer);
        return true;
    }

    private static CustomerResponse ToResponse(Customer c) =>
        new(c.Id, c.Name, c.Email, c.Phone, c.Company, c.Status, c.Notes, c.CreatedAt, c.UpdatedAt);
}
