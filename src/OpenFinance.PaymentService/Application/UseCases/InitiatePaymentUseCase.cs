using OpenFinance.PaymentService.Domain.Entities;
using OpenFinance.PaymentService.Domain.Repositories;
using OpenFinance.Shared.Contracts;
using OpenFinance.Shared.Results;

namespace OpenFinance.PaymentService.Application.UseCases;

public sealed class InitiatePaymentUseCase(IPaymentRepository repository)
{
    public async Task<Result<PaymentResponse>> ExecuteAsync(
        InitiatePaymentRequest request,
        CancellationToken ct = default)
    {
        try
        {
            var payment = Payment.Create(
                request.ConsentId,
                request.DebtorAccountId,
                request.CreditorAccountId,
                request.CreditorName,
                request.CreditorCpfCnpj,
                request.Amount,
                request.Currency,
                request.Description,
                request.Type);

            await repository.AddAsync(payment, ct);
            return Result.Success(MapToResponse(payment));
        }
        catch (ArgumentException ex)
        {
            return Result.Failure<PaymentResponse>(ex.Message);
        }
    }

    internal static PaymentResponse MapToResponse(Payment payment) =>
        new(payment.Id,
            payment.ConsentId,
            payment.Status,
            payment.Amount,
            payment.Currency,
            payment.Description,
            payment.CreatedAt,
            payment.CompletedAt);
}
