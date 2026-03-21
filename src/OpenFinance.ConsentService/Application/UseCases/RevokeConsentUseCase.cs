using Microsoft.Extensions.Logging;
using OpenFinance.ConsentService.Domain.Repositories;
using OpenFinance.Shared.Results;

namespace OpenFinance.ConsentService.Application.UseCases;

public sealed class RevokeConsentUseCase(
    IConsentRepository repository,
    ILogger<RevokeConsentUseCase> logger)
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

            logger.LogInformation(
                "AUDIT: Consent revoked - ConsentId={ConsentId}, ClientId={ClientId}, UserId={UserId}, Reason={Reason}",
                consentId, consent.ClientId, consent.UserId, reason);

            return Result.Success();
        }
        catch (InvalidOperationException ex)
        {
            logger.LogWarning("AUDIT: Consent revocation failed - ConsentId={ConsentId}, Error={Error}",
                consentId, ex.Message);
            return Result.Failure(ex.Message);
        }
    }
}
