using OpenFinance.AccountService.Domain.Repositories;
using OpenFinance.Shared.Consent;
using OpenFinance.Shared.Contracts;
using OpenFinance.Shared.Results;

namespace OpenFinance.AccountService.Application.UseCases;

public sealed class GetTransactionsUseCase(IAccountRepository repository, IConsentValidator consentValidator)
{
    public async Task<Result<TransactionListResponse>> ExecuteAsync(
        Guid accountId,
        Guid consentId,
        DateOnly from,
        DateOnly to,
        CancellationToken ct = default)
    {
        if (from > to)
            return Result.Failure<TransactionListResponse>("'from' date must be before or equal to 'to' date.");

        var validation = await consentValidator.ValidateAsync(
            consentId, [OpenFinancePermissions.AccountsTransactionsRead], ct);
        if (!validation.IsValid)
            return Result.Failure<TransactionListResponse>(validation.ErrorMessage!);

        var account = await repository.GetByIdAsync(accountId, ct);
        if (account is null)
            return Result.Failure<TransactionListResponse>($"Account {accountId} not found.");

        var transactions = await repository.GetTransactionsAsync(accountId, from, to, ct);
        var summaries = transactions
            .Select(t => new TransactionSummary(
                t.Id, t.CompletedDateTime, t.Amount, t.Currency, t.Type, t.Description))
            .ToList();

        return Result.Success(new TransactionListResponse(summaries));
    }
}
