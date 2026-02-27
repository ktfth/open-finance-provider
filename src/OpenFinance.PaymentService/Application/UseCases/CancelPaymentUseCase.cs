using OpenFinance.PaymentService.Domain.Repositories;
using OpenFinance.Shared.Results;

namespace OpenFinance.PaymentService.Application.UseCases;

public sealed class CancelPaymentUseCase(IPaymentRepository repository)
{
    public async Task<Result> ExecuteAsync(Guid paymentId, string reason, CancellationToken ct = default)
    {
        var payment = await repository.GetByIdAsync(paymentId, ct);
        if (payment is null)
            return Result.Failure($"Payment {paymentId} not found.");

        try
        {
            payment.Cancel(reason);
            await repository.UpdateAsync(payment, ct);
            return Result.Success();
        }
        catch (InvalidOperationException ex)
        {
            return Result.Failure(ex.Message);
        }
    }
}
