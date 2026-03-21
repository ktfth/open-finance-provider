using Microsoft.EntityFrameworkCore;
using OpenFinance.CustomerService.Domain.Entities;
using OpenFinance.CustomerService.Domain.Repositories;

namespace OpenFinance.CustomerService.Infrastructure.Persistence;

public class CustomerRepository(CustomerDbContext context) : ICustomerRepository
{
    public async Task<PersonalCustomer?> GetPersonalByUserIdAsync(string userId, CancellationToken ct = default) =>
        await context.PersonalCustomers.FirstOrDefaultAsync(c => c.UserId == userId, ct);

    public async Task<PersonalQualification?> GetPersonalQualificationAsync(string userId, CancellationToken ct = default) =>
        await context.PersonalQualifications.FirstOrDefaultAsync(q => q.UserId == userId, ct);

    public async Task<IReadOnlyList<CustomerAddress>> GetAddressesAsync(string userId, CancellationToken ct = default) =>
        await context.CustomerAddresses.Where(a => a.UserId == userId).ToListAsync(ct);

    public async Task<IReadOnlyList<CustomerContact>> GetContactsAsync(string userId, CancellationToken ct = default) =>
        await context.CustomerContacts.Where(c => c.UserId == userId).ToListAsync(ct);

    public async Task<BusinessCustomer?> GetBusinessByUserIdAsync(string userId, CancellationToken ct = default) =>
        await context.BusinessCustomers.FirstOrDefaultAsync(c => c.UserId == userId, ct);

    public async Task AddPersonalAsync(PersonalCustomer customer, CancellationToken ct = default)
    {
        await context.PersonalCustomers.AddAsync(customer, ct);
        await context.SaveChangesAsync(ct);
    }

    public async Task AddBusinessAsync(BusinessCustomer customer, CancellationToken ct = default)
    {
        await context.BusinessCustomers.AddAsync(customer, ct);
        await context.SaveChangesAsync(ct);
    }
}
