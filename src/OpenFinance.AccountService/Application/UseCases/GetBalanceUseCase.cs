using OpenFinance.AccountService.Domain.Repositories;
using OpenFinance.Shared.Consent;
using OpenFinance.Shared.Contracts;
using OpenFinance.Shared.Results;

namespace OpenFinance.AccountService.Application.UseCases;

public sealed class GetBalanceUseCase(IAccountRepository repository, IConsentValidator consentValidator)
{
    public async Task<Result<BalanceResponse?>> ExecuteAsync(
        Guid accountId,
        Guid consentId,
        CancellationToken ct = default)
    {
        var validation = await consentValidator.ValidateAsync(
            consentId, [OpenFinancePermissions.AccountsBalancesRead], ct);
        if (!validation.IsValid)
            return Result.Failure<BalanceResponse?>(validation.ErrorMessage!);

        var account = await repository.GetByIdAsync(accountId, ct);
        if (account is null || !account.IsActive)
            return Result.Success<BalanceResponse?>(null);

        var balance = await repository.GetBalanceAsync(accountId, ct);
        if (balance is null)
            return Result.Success<BalanceResponse?>(null);

        return Result.Success<BalanceResponse?>(new BalanceResponse(
            accountId,
            balance.AvailableBalance,
            balance.BlockedBalance,
            balance.Currency,
            balance.UpdatedAt));
    }
}
