using OpenFinance.ConsentService.Domain.Entities;
using OpenFinance.ConsentService.Domain.Repositories;
using OpenFinance.Shared.Contracts;
using OpenFinance.Shared.Results;

namespace OpenFinance.ConsentService.Application.UseCases;

public sealed class CreateConsentUseCase(IConsentRepository repository)
{
    public async Task<Result<ConsentResponse>> ExecuteAsync(
        CreateConsentRequest request,
        CancellationToken ct = default)
    {
        try
        {
            var consent = Consent.Create(
                request.ClientId,
                request.UserId,
                request.Permissions,
                request.ExpiresAt,
                request.RedirectUri);

            await repository.AddAsync(consent, ct);

            return Result.Success(MapToResponse(consent));
        }
        catch (ArgumentException ex)
        {
            return Result.Failure<ConsentResponse>(ex.Message);
        }
    }

    internal static ConsentResponse MapToResponse(Consent consent) =>
        new(consent.Id,
            consent.ClientId,
            consent.UserId,
            [.. consent.Permissions],
            consent.Status,
            consent.CreatedAt,
            consent.ExpiresAt);
}
