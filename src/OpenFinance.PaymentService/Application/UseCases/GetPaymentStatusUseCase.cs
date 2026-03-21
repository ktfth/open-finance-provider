using OpenFinance.PaymentService.Domain.Repositories;
using OpenFinance.Shared.Consent;
using OpenFinance.Shared.Contracts;
using OpenFinance.Shared.Results;

namespace OpenFinance.PaymentService.Application.UseCases;

public sealed class GetPaymentStatusUseCase(IPaymentRepository repository, IConsentValidator consentValidator)
{
    public async Task<Result<PaymentResponse?>> ExecuteAsync(Guid consentId, Guid paymentId, CancellationToken ct = default)
    {
        var validation = await consentValidator.ValidateAsync(
            consentId, [OpenFinance.Shared.Consent.OpenFinancePermissions.PaymentsRead], ct);
        if (!validation.IsValid)
            return Result.Failure<PaymentResponse?>(validation.ErrorMessage!);

        var payment = await repository.GetByIdAsync(paymentId, ct);
        if (payment is null)
            return Result.Success<PaymentResponse?>(null);

        // Ownership check: payment must belong to the referenced consent
        if (payment.ConsentId != consentId)
            return Result.Failure<PaymentResponse?>("CONSENT_MISMATCH: Payment does not belong to the provided consent.");

        return Result.Success<PaymentResponse?>(InitiatePaymentUseCase.MapToResponse(payment));
    }
}
