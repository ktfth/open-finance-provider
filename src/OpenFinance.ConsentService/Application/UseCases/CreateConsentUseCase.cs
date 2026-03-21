using Microsoft.Extensions.Logging;
using OpenFinance.ConsentService.Domain.Entities;
using OpenFinance.ConsentService.Domain.Repositories;
using OpenFinance.Shared.Contracts;
using OpenFinance.Shared.Results;

namespace OpenFinance.ConsentService.Application.UseCases;

public sealed class CreateConsentUseCase(
    IConsentRepository repository,
    ILogger<CreateConsentUseCase> logger)
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

            logger.LogInformation(
                "AUDIT: Consent created - ConsentId={ConsentId}, ClientId={ClientId}, UserId={UserId}, Permissions=[{Permissions}], ExpiresAt={ExpiresAt}",
                consent.Id, request.ClientId, request.UserId, string.Join(", ", request.Permissions), request.ExpiresAt);

            return Result.Success(MapToResponse(consent));
        }
        catch (ArgumentException ex)
        {
            logger.LogWarning("AUDIT: Consent creation failed - ClientId={ClientId}, Error={Error}",
                request.ClientId, ex.Message);
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
