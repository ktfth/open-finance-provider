using OpenFinance.PensionService.Domain.Repositories;
using OpenFinance.Shared.Consent;
using OpenFinance.Shared.Contracts;
using OpenFinance.Shared.Results;

namespace OpenFinance.PensionService.Application.UseCases;

/// <summary>
/// Returns all withdrawal and redemption events for a pension plan.
/// Includes gross amount, withholding tax, and net amount for each event.
/// </summary>
public sealed class GetPensionWithdrawalsUseCase(IPensionRepository repository, IConsentValidator consentValidator)
{
    public async Task<Result<PensionWithdrawalListResponse>> ExecuteAsync(
        Guid pensionId, Guid consentId, CancellationToken ct = default)
    {
        var validation = await consentValidator.ValidateAsync(
            consentId, [OpenFinancePermissions.PensionsRead], ct);
        if (!validation.IsValid)
            return Result.Failure<PensionWithdrawalListResponse>(validation.ErrorMessage!);

        var withdrawals = await repository.GetWithdrawalsAsync(pensionId, ct);

        var summaries = withdrawals.Select(w => new PensionWithdrawalSummary(
            w.Id,
            w.WithdrawalDate,
            w.GrossAmount,
            w.TaxAmount,
            w.NetAmount,
            w.Currency,
            w.Type
        )).ToList();

        return Result.Success(new PensionWithdrawalListResponse(summaries));
    }
}
