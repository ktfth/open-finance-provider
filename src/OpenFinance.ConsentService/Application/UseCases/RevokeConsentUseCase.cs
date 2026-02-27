using OpenFinance.ConsentService.Domain.Repositories;
using OpenFinance.Shared.Results;

namespace OpenFinance.ConsentService.Application.UseCases;

public sealed class RevokeConsentUseCase(IConsentRepository repository)
{
    public async Task<Result> ExecuteAsync(Guid consentId, string reason, CancellationToken ct = default)
    {
        var consent = await repository.GetByIdAsync(consentId, ct);
        if (consent is null)
            return Result.Failure($"Consent {consentId} not found.");

        try
        {
            consent.Revoke(reason);
            await repository.UpdateAsync(consent, ct);
            return Result.Success();
        }
        catch (InvalidOperationException ex)
        {
            return Result.Failure(ex.Message);
        }
    }
}
