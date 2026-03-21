using OpenFinance.Shared.Contracts;
using OpenFinance.Shared.Domain;

namespace OpenFinance.CustomerService.Domain.Entities;

public class CustomerAddress : Entity
{
    private CustomerAddress() { }

    public static CustomerAddress Create(
        string userId,
        string street,
        string number,
        string? complement,
        string district,
        string city,
        string state,
        string postalCode,
        string country,
        AddressType addressType)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(userId);
        ArgumentException.ThrowIfNullOrWhiteSpace(street);
        ArgumentException.ThrowIfNullOrWhiteSpace(number);
        ArgumentException.ThrowIfNullOrWhiteSpace(district);
        ArgumentException.ThrowIfNullOrWhiteSpace(city);
        ArgumentException.ThrowIfNullOrWhiteSpace(state);
        ArgumentException.ThrowIfNullOrWhiteSpace(postalCode);
        ArgumentException.ThrowIfNullOrWhiteSpace(country);

        return new CustomerAddress
        {
            UserId = userId,
            Street = street,
            Number = number,
            Complement = complement,
            District = district,
            City = city,
            State = state,
            PostalCode = postalCode,
            Country = country,
            AddressType = addressType
        };
    }

    public string UserId { get; private set; } = default!;
    public string Street { get; private set; } = default!;
    public string Number { get; private set; } = default!;
    public string? Complement { get; private set; }
    public string District { get; private set; } = default!;
    public string City { get; private set; } = default!;
    public string State { get; private set; } = default!;
    public string PostalCode { get; private set; } = default!;
    public string Country { get; private set; } = default!;
    public AddressType AddressType { get; private set; }
}
