using OpenFinance.PensionService.Domain.Repositories;
using OpenFinance.Shared.Consent;
using OpenFinance.Shared.Contracts;
using OpenFinance.Shared.Results;

namespace OpenFinance.PensionService.Application.UseCases;

/// <summary>
/// Returns all contribution records for a pension plan, ordered by most recent first.
/// Includes regular, extra, portability-in, and employer contributions.
/// </summary>
public sealed class GetPensionContributionsUseCase(IPensionRepository repository, IConsentValidator consentValidator)
{
    public async Task<Result<PensionContributionListResponse>> ExecuteAsync(
        Guid pensionId, Guid consentId, CancellationToken ct = default)
    {
        var validation = await consentValidator.ValidateAsync(
            consentId, [OpenFinancePermissions.PensionsRead], ct);
        if (!validation.IsValid)
            return Result.Failure<PensionContributionListResponse>(validation.ErrorMessage!);

        var contributions = await repository.GetContributionsAsync(pensionId, ct);

        var summaries = contributions.Select(c => new PensionContributionSummary(
            c.Id,
            c.ContributionDate,
            c.Amount,
            c.Currency,
            c.Type,
            c.Status
        )).ToList();

        return Result.Success(new PensionContributionListResponse(summaries));
    }
}
