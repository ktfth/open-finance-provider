using OpenFinance.AccountService.Domain.Repositories;
using OpenFinance.Shared.Consent;
using OpenFinance.Shared.Contracts;
using OpenFinance.Shared.Results;

namespace OpenFinance.AccountService.Application.UseCases;

public sealed class GetAccountsUseCase(IAccountRepository repository, IConsentValidator consentValidator)
{
    public async Task<Result<AccountListResponse>> ExecuteAsync(
        string userId,
        Guid consentId,
        CancellationToken ct = default)
    {
        var validation = await consentValidator.ValidateAsync(
            consentId, [OpenFinancePermissions.AccountsRead], ct);
        if (!validation.IsValid)
            return Result.Failure<AccountListResponse>(validation.ErrorMessage!);

        var accounts = await repository.GetByUserIdAsync(userId, ct);
        var summaries = accounts
            .Where(a => a.IsActive)
            .Select(a => new AccountSummary(a.Id, a.AccountNumber, a.BranchCode, a.Type, a.Currency))
            .ToList();

        return Result.Success(new AccountListResponse(summaries));
    }
}
