using OpenFinance.ExchangeService.Domain.Repositories;
using OpenFinance.Shared.Consent;
using OpenFinance.Shared.Contracts;
using OpenFinance.Shared.Results;

namespace OpenFinance.ExchangeService.Application.UseCases;

public sealed class GetExchangeEventsUseCase(IExchangeRepository repository, IConsentValidator consentValidator)
{
    public async Task<Result<ExchangeEventListResponse>> ExecuteAsync(
        Guid operationId, Guid consentId, CancellationToken ct = default)
    {
        var validation = await consentValidator.ValidateAsync(
            consentId, [OpenFinancePermissions.ExchangesRead], ct);
        if (!validation.IsValid)
            return Result.Failure<ExchangeEventListResponse>(validation.ErrorMessage!);

        var events = await repository.GetEventsAsync(operationId, ct);
        var summaries = events.Select(e => new ExchangeEventSummary(
            e.Id,
            e.EventType,
            e.EventDate,
            e.ForeignCurrencyAmount,
            e.LocalCurrencyAmount,
            e.ForeignCurrency,
            e.LocalCurrency,
            e.ExchangeRate
        )).ToList();

        return Result.Success(new ExchangeEventListResponse(summaries));
    }
}
