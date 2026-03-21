using OpenFinance.Shared.Contracts;
using OpenFinance.Shared.Domain;

namespace OpenFinance.CustomerService.Domain.Entities;

public class CustomerContact : Entity
{
    private CustomerContact() { }

    public static CustomerContact Create(
        string userId,
        PhoneType phoneType,
        string countryCallingCode,
        string areaCode,
        string phoneNumber,
        string email,
        bool isMainEmail)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(userId);
        ArgumentException.ThrowIfNullOrWhiteSpace(countryCallingCode);
        ArgumentException.ThrowIfNullOrWhiteSpace(areaCode);
        ArgumentException.ThrowIfNullOrWhiteSpace(phoneNumber);
        ArgumentException.ThrowIfNullOrWhiteSpace(email);

        return new CustomerContact
        {
            UserId = userId,
            PhoneType = phoneType,
            CountryCallingCode = countryCallingCode,
            AreaCode = areaCode,
            PhoneNumber = phoneNumber,
            Email = email,
            IsMainEmail = isMainEmail
        };
    }

    public string UserId { get; private set; } = default!;
    public PhoneType PhoneType { get; private set; }
    public string CountryCallingCode { get; private set; } = default!;
    public string AreaCode { get; private set; } = default!;
    public string PhoneNumber { get; private set; } = default!;
    public string Email { get; private set; } = default!;
    public bool IsMainEmail { get; private set; }
}
