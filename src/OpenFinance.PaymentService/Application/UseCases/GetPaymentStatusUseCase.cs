using OpenFinance.PaymentService.Domain.Repositories;
using OpenFinance.Shared.Contracts;
using OpenFinance.Shared.Results;

namespace OpenFinance.PaymentService.Application.UseCases;

public sealed class GetPaymentStatusUseCase(IPaymentRepository repository)
{
    public async Task<Result<PaymentResponse?>> ExecuteAsync(Guid paymentId, CancellationToken ct = default)
    {
        var payment = await repository.GetByIdAsync(paymentId, ct);
        if (payment is null)
            return Result.Success<PaymentResponse?>(null);

        return Result.Success<PaymentResponse?>(InitiatePaymentUseCase.MapToResponse(payment));
    }
}
