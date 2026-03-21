using OpenFinance.ResourcesService.Domain.Repositories;
using OpenFinance.Shared.Consent;
using OpenFinance.Shared.Contracts;
using OpenFinance.Shared.Results;

namespace OpenFinance.ResourcesService.Application.UseCases;

public sealed class GetResourcesUseCase(IResourceRepository repository, IConsentValidator consentValidator)
{
    public async Task<Result<ResourceListResponse>> ExecuteAsync(
        Guid consentId,
        CancellationToken ct = default)
    {
        var validation = await consentValidator.ValidateAsync(
            consentId, [OpenFinancePermissions.ResourcesRead], ct);
        if (!validation.IsValid)
            return Result.Failure<ResourceListResponse>(validation.ErrorMessage!);

        var resources = await repository.GetByConsentIdAsync(consentId, ct);
        var summaries = resources
            .Select(r => new ResourceSummary(r.ResourceId, r.Type, r.Status))
            .ToList();

        return Result.Success(new ResourceListResponse(summaries));
    }
}
