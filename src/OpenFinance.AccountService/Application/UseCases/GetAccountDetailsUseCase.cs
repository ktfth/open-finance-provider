using OpenFinance.AccountService.Domain.Repositories;
using OpenFinance.Shared.Consent;
using OpenFinance.Shared.Contracts;
using OpenFinance.Shared.Results;

namespace OpenFinance.AccountService.Application.UseCases;

public sealed class GetAccountDetailsUseCase(IAccountRepository repository, IConsentValidator consentValidator)
{
    public async Task<Result<AccountDetailsResponse?>> ExecuteAsync(
        Guid accountId,
        Guid consentId,
        CancellationToken ct = default)
    {
        var validation = await consentValidator.ValidateAsync(
            consentId, [OpenFinancePermissions.AccountsRead], ct);
        if (!validation.IsValid)
            return Result.Failure<AccountDetailsResponse?>(validation.ErrorMessage!);

        var account = await repository.GetByIdAsync(accountId, ct);
        if (account is null || !account.IsActive)
            return Result.Success<AccountDetailsResponse?>(null);

        return Result.Success<AccountDetailsResponse?>(new AccountDetailsResponse(
            account.Id,
            account.AccountNumber,
            account.BranchCode,
            account.Type,
            account.Currency,
            account.OwnerName,
            account.Cpf));
    }
}
