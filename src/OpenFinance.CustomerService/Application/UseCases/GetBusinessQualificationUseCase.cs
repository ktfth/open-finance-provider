using OpenFinance.Shared.Consent;
using OpenFinance.Shared.Contracts;
using OpenFinance.Shared.Results;

namespace OpenFinance.CustomerService.Application.UseCases;

public sealed class GetBusinessQualificationUseCase(IConsentValidator consentValidator)
{
    public async Task<Result<BusinessQualificationResponse?>> ExecuteAsync(
        string userId,
        Guid consentId,
        CancellationToken ct = default)
    {
        var validation = await consentValidator.ValidateAsync(
            consentId, [OpenFinancePermissions.CustomersBusinessAdittionalInfoRead], ct);
        if (!validation.IsValid)
            return Result.Failure<BusinessQualificationResponse?>(validation.ErrorMessage!);

        // Placeholder - returns null for now
        await Task.CompletedTask;
        return Result.Success<BusinessQualificationResponse?>(null);
    }
}
