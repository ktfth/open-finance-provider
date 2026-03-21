using OpenFinance.CustomerService.Domain.Repositories;
using OpenFinance.Shared.Consent;
using OpenFinance.Shared.Contracts;
using OpenFinance.Shared.Results;

namespace OpenFinance.CustomerService.Application.UseCases;

public sealed class GetPersonalIdentificationUseCase(ICustomerRepository repository, IConsentValidator consentValidator)
{
    public async Task<Result<PersonalIdentificationResponse?>> ExecuteAsync(
        string userId,
        Guid consentId,
        CancellationToken ct = default)
    {
        var validation = await consentValidator.ValidateAsync(
            consentId, [OpenFinancePermissions.CustomersPersonalIdentificationsRead], ct);
        if (!validation.IsValid)
            return Result.Failure<PersonalIdentificationResponse?>(validation.ErrorMessage!);

        var customer = await repository.GetPersonalByUserIdAsync(userId, ct);
        if (customer is null)
            return Result.Success<PersonalIdentificationResponse?>(null);

        var addresses = await repository.GetAddressesAsync(userId, ct);
        var contacts = await repository.GetContactsAsync(userId, ct);

        var addressRecords = addresses
            .Select(a => new CustomerAddress(
                a.Street,
                a.Number,
                a.Complement,
                a.District,
                a.City,
                a.State,
                a.PostalCode,
                a.Country,
                a.AddressType))
            .ToList();

        var phoneRecords = contacts
            .Select(c => new CustomerPhone(
                c.PhoneType,
                c.CountryCallingCode,
                c.AreaCode,
                c.PhoneNumber))
            .ToList();

        var emailRecords = contacts
            .Select(c => new CustomerEmail(c.Email, c.IsMainEmail))
            .ToList();

        var response = new PersonalIdentificationResponse(
            customer.UserId,
            customer.CpfNumber,
            customer.SocialName,
            customer.BirthDate,
            customer.MaritalStatus,
            customer.Sex,
            customer.Nationality,
            customer.BirthCountry,
            Documents: Array.Empty<CustomerDocument>(),
            Addresses: addressRecords,
            Phones: phoneRecords,
            Emails: emailRecords);

        return Result.Success<PersonalIdentificationResponse?>(response);
    }
}
