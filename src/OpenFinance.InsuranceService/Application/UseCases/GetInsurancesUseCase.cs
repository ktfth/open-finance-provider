using OpenFinance.InsuranceService.Domain.Repositories;
using OpenFinance.Shared.Consent;
using OpenFinance.Shared.Contracts;
using OpenFinance.Shared.Results;

namespace OpenFinance.InsuranceService.Application.UseCases;

/// <summary>
/// Returns the list of insurance policies for an authenticated user.
/// Requires permission: INSURANCE_READ
/// </summary>
public sealed class GetInsurancesUseCase(IInsuranceRepository repository, IConsentValidator consentValidator)
{
    public async Task<Result<InsuranceListResponse>> ExecuteAsync(
        string userId,
        Guid consentId,
        CancellationToken ct = default)
    {
        var validation = await consentValidator.ValidateAsync(
            consentId, [OpenFinancePermissions.InsurancesRead], ct);
        if (!validation.IsValid)
            return Result.Failure<InsuranceListResponse>(validation.ErrorMessage!);

        ArgumentException.ThrowIfNullOrWhiteSpace(userId);

        var insurances = await repository.GetByUserIdAsync(userId, ct);

        var summaries = insurances
            .Select(i => new InsuranceSummary(
                i.Id,
                i.Type,
                i.ProductName,
                i.InsurerName,
                i.InsurerCnpj,
                i.Status,
                i.PolicyNumber,
                i.Currency))
            .ToList();

        return Result.Success(new InsuranceListResponse(summaries));
    }
}
