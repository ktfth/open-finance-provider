using OpenFinance.ConsentService.Domain.Repositories;
using OpenFinance.Shared.Results;

namespace OpenFinance.ConsentService.Application.UseCases;

public sealed class ValidateConsentUseCase(IConsentRepository repository)
{
    public async Task<Result<bool>> ExecuteAsync(
        Guid consentId,
        string[] requiredPermissions,
        CancellationToken ct = default)
    {
        var consent = await repository.GetByIdAsync(consentId, ct);
        if (consent is null)
            return Result.Success(false);

        var isValid = consent.IsActive() && consent.HasPermissions(requiredPermissions);
        return Result.Success(isValid);
    }
}
