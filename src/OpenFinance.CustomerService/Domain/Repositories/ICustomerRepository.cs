using OpenFinance.CustomerService.Domain.Entities;

namespace OpenFinance.CustomerService.Domain.Repositories;

public interface ICustomerRepository
{
    Task<PersonalCustomer?> GetPersonalByUserIdAsync(string userId, CancellationToken ct = default);
    Task<PersonalQualification?> GetPersonalQualificationAsync(string userId, CancellationToken ct = default);
    Task<IReadOnlyList<CustomerAddress>> GetAddressesAsync(string userId, CancellationToken ct = default);
    Task<IReadOnlyList<CustomerContact>> GetContactsAsync(string userId, CancellationToken ct = default);
    Task<BusinessCustomer?> GetBusinessByUserIdAsync(string userId, CancellationToken ct = default);
    Task AddPersonalAsync(PersonalCustomer customer, CancellationToken ct = default);
    Task AddBusinessAsync(BusinessCustomer customer, CancellationToken ct = default);
}
