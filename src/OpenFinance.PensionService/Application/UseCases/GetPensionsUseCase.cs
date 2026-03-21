using OpenFinance.PensionService.Domain.Repositories;
using OpenFinance.Shared.Consent;
using OpenFinance.Shared.Contracts;
using OpenFinance.Shared.Results;

namespace OpenFinance.PensionService.Application.UseCases;

/// <summary>
/// Returns all pension plans associated with a given user, subject to consent validation.
/// </summary>
public sealed class GetPensionsUseCase(IPensionRepository repository, IConsentValidator consentValidator)
{
    public async Task<Result<PensionListResponse>> ExecuteAsync(
        string userId, Guid consentId, CancellationToken ct = default)
    {
        var validation = await consentValidator.ValidateAsync(
            consentId, [OpenFinancePermissions.PensionsRead], ct);
        if (!validation.IsValid)
            return Result.Failure<PensionListResponse>(validation.ErrorMessage!);

        ArgumentException.ThrowIfNullOrWhiteSpace(userId);

        var pensions = await repository.GetByUserIdAsync(userId, ct);

        var summaries = pensions.Select(p => new PensionSummary(
            p.Id,
            p.Type,
            p.Modality,
            p.ProductName,
            p.InsurerName,
            p.InsurerCnpj,
            p.Status,
            p.CertificateNumber,
            p.Currency
        )).ToList();

        return Result.Success(new PensionListResponse(summaries));
    }
}
