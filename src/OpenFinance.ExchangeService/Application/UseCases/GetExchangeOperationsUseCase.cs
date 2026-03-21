using OpenFinance.ExchangeService.Domain.Repositories;
using OpenFinance.Shared.Consent;
using OpenFinance.Shared.Contracts;
using OpenFinance.Shared.Results;

namespace OpenFinance.ExchangeService.Application.UseCases;

public sealed class GetExchangeOperationsUseCase(IExchangeRepository repository, IConsentValidator consentValidator)
{
    public async Task<Result<ExchangeOperationListResponse>> ExecuteAsync(
        string userId, Guid consentId, CancellationToken ct = default)
    {
        var validation = await consentValidator.ValidateAsync(
            consentId, [OpenFinancePermissions.ExchangesRead], ct);
        if (!validation.IsValid)
            return Result.Failure<ExchangeOperationListResponse>(validation.ErrorMessage!);

        ArgumentException.ThrowIfNullOrWhiteSpace(userId);

        var operations = await repository.GetByUserIdAsync(userId, ct);
        var summaries = operations.Select(o => new ExchangeOperationSummary(
            o.Id,
            o.OperationNumber,
            o.OperationType,
            o.Category,
            o.Status,
            o.ForeignCurrency,
            o.LocalCurrency,
            o.OperationDate,
            o.ForeignCurrencyAmount,
            o.LocalCurrencyAmount
        )).ToList();

        return Result.Success(new ExchangeOperationListResponse(summaries));
    }
}
