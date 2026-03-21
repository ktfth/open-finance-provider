using OpenFinance.CustomerService.Domain.Repositories;
using OpenFinance.Shared.Consent;
using OpenFinance.Shared.Contracts;
using OpenFinance.Shared.Results;

namespace OpenFinance.CustomerService.Application.UseCases;

public sealed class GetBusinessIdentificationUseCase(ICustomerRepository repository, IConsentValidator consentValidator)
{
    public async Task<Result<BusinessIdentificationResponse?>> ExecuteAsync(
        string userId,
        Guid consentId,
        CancellationToken ct = default)
    {
        var validation = await consentValidator.ValidateAsync(
            consentId, [OpenFinancePermissions.CustomersBusinessIdentificationsRead], ct);
        if (!validation.IsValid)
            return Result.Failure<BusinessIdentificationResponse?>(validation.ErrorMessage!);

        var customer = await repository.GetBusinessByUserIdAsync(userId, ct);
        if (customer is null)
            return Result.Success<BusinessIdentificationResponse?>(null);

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

        var response = new BusinessIdentificationResponse(
            customer.UserId,
            customer.CnpjNumber,
            customer.CompanyName,
            customer.TradeName,
            customer.IncorporationDate,
            Addresses: addressRecords,
            Phones: phoneRecords,
            Emails: emailRecords,
            Partners: Array.Empty<BusinessPartner>());

        return Result.Success<BusinessIdentificationResponse?>(response);
    }
}
