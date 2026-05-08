using AuthJWT.Api.Data;
using AuthJWT.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace AuthJWT.Api.Repositories;

public class CustomerRepository(AppDbContext db) : ICustomerRepository
{
    public async Task<Customer> CreateAsync(Customer customer)
    {
        db.Customers.Add(customer);
        await db.SaveChangesAsync();
        return customer;
    }

    public Task<Customer?> GetByIdAsync(int id) =>
        db.Customers.FirstOrDefaultAsync(c => c.Id == id && !c.IsDeleted);

    public async Task<(IEnumerable<Customer> Items, int TotalCount)> GetPagedAsync(
        int page, int pageSize, string? search, string? status)
    {
        var query = db.Customers.Where(c => !c.IsDeleted);

        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(c =>
                c.Name.Contains(search) ||
                c.Email.Contains(search) ||
                c.Company.Contains(search));

        if (!string.IsNullOrWhiteSpace(status))
            query = query.Where(c => c.Status == status);

        var total = await query.CountAsync();
        var items = await query
            .OrderBy(c => c.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, total);
    }

    public async Task<Customer> UpdateAsync(Customer customer)
    {
        customer.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync();
        return customer;
    }

    public async Task SoftDeleteAsync(Customer customer)
    {
        customer.IsDeleted = true;
        customer.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync();
    }

    public Task<bool> EmailExistsAsync(string email, int? excludeId = null) =>
        db.Customers.AnyAsync(c =>
            c.Email == email &&
            !c.IsDeleted &&
            (excludeId == null || c.Id != excludeId));
}
