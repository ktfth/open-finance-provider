using OpenFinance.PensionService.Domain.Repositories;
using OpenFinance.Shared.Consent;
using OpenFinance.Shared.Contracts;
using OpenFinance.Shared.Results;

namespace OpenFinance.PensionService.Application.UseCases;

/// <summary>
/// Returns the current financial balance of a pension plan at the latest reference date.
/// Includes gross/net balance, total contributions, yield, and fees applied.
/// </summary>
public sealed class GetPensionBalanceUseCase(IPensionRepository repository, IConsentValidator consentValidator)
{
    public async Task<Result<PensionBalanceResponse?>> ExecuteAsync(
        Guid pensionId, Guid consentId, CancellationToken ct = default)
    {
        var validation = await consentValidator.ValidateAsync(
            consentId, [OpenFinancePermissions.PensionsRead], ct);
        if (!validation.IsValid)
            return Result.Failure<PensionBalanceResponse?>(validation.ErrorMessage!);

        var pension = await repository.GetByIdAsync(pensionId, ct);
        if (pension is null)
            return Result.Success<PensionBalanceResponse?>(null);

        var balance = await repository.GetBalanceAsync(pensionId, ct);
        if (balance is null)
            return Result.Success<PensionBalanceResponse?>(null);

        var response = new PensionBalanceResponse(
            pensionId,
            balance.ReferenceDate,
            balance.GrossBalance,
            balance.NetBalance,
            balance.TotalContributions,
            balance.TotalYield,
            balance.ManagementFee,
            balance.LoadingFee,
            balance.Currency
        );

        return Result.Success<PensionBalanceResponse?>(response);
    }
}
