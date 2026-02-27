using OpenFinance.ConsentService.Domain.Repositories;
using OpenFinance.Shared.Contracts;
using OpenFinance.Shared.Results;

namespace OpenFinance.ConsentService.Application.UseCases;

public sealed class GetConsentUseCase(IConsentRepository repository)
{
    public async Task<Result<ConsentResponse?>> ExecuteAsync(Guid consentId, CancellationToken ct = default)
    {
        var consent = await repository.GetByIdAsync(consentId, ct);
        if (consent is null)
            return Result.Success<ConsentResponse?>(null);

        return Result.Success<ConsentResponse?>(CreateConsentUseCase.MapToResponse(consent));
    }
}
