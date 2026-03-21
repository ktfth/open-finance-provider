using OpenFinance.ExchangeService.Domain.Repositories;
using OpenFinance.Shared.Consent;
using OpenFinance.Shared.Contracts;
using OpenFinance.Shared.Results;

namespace OpenFinance.ExchangeService.Application.UseCases;

public sealed class GetExchangeOperationDetailsUseCase(IExchangeRepository repository, IConsentValidator consentValidator)
{
    public async Task<Result<ExchangeOperationDetailsResponse?>> ExecuteAsync(
        Guid operationId, Guid consentId, CancellationToken ct = default)
    {
        var validation = await consentValidator.ValidateAsync(
            consentId, [OpenFinancePermissions.ExchangesRead], ct);
        if (!validation.IsValid)
            return Result.Failure<ExchangeOperationDetailsResponse?>(validation.ErrorMessage!);

        var operation = await repository.GetByIdAsync(operationId, ct);
        if (operation is null)
            return Result.Success<ExchangeOperationDetailsResponse?>(null);

        var response = new ExchangeOperationDetailsResponse(
            operation.Id,
            operation.OperationNumber,
            operation.OperationType,
            operation.Category,
            operation.Status,
            operation.ForeignCurrency,
            operation.LocalCurrency,
            operation.OperationDate,
            operation.DeliveryDate,
            operation.ForeignCurrencyAmount,
            operation.LocalCurrencyAmount,
            operation.ExchangeRate,
            operation.VETAmount,
            operation.IOFAmount,
            operation.IRAmount,
            operation.CounterpartyName,
            operation.CounterpartyCountry,
            operation.DeliveryType
        );

        return Result.Success<ExchangeOperationDetailsResponse?>(response);
    }
}
