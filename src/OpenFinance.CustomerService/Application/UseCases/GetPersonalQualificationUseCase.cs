using OpenFinance.CustomerService.Domain.Repositories;
using OpenFinance.Shared.Consent;
using OpenFinance.Shared.Contracts;
using OpenFinance.Shared.Results;

namespace OpenFinance.CustomerService.Application.UseCases;

public sealed class GetPersonalQualificationUseCase(ICustomerRepository repository, IConsentValidator consentValidator)
{
    public async Task<Result<PersonalQualificationResponse?>> ExecuteAsync(
        string userId,
        Guid consentId,
        CancellationToken ct = default)
    {
        var validation = await consentValidator.ValidateAsync(
            consentId, [OpenFinancePermissions.CustomersPersonalAdittionalInfoRead], ct);
        if (!validation.IsValid)
            return Result.Failure<PersonalQualificationResponse?>(validation.ErrorMessage!);

        var qualification = await repository.GetPersonalQualificationAsync(userId, ct);
        if (qualification is null)
            return Result.Success<PersonalQualificationResponse?>(null);

        var response = new PersonalQualificationResponse(
            qualification.UserId,
            qualification.OccupationType,
            qualification.OccupationDescription,
            qualification.InformedIncomeFrequency,
            qualification.InformedIncomeAmount,
            qualification.InformedIncomeCurrency,
            qualification.InformedIncomeDate);

        return Result.Success<PersonalQualificationResponse?>(response);
    }
}
