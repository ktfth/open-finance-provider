using Microsoft.Extensions.Logging;
using OpenFinance.PaymentService.Domain.Repositories;
using OpenFinance.Shared.Consent;
using OpenFinance.Shared.Results;

namespace OpenFinance.PaymentService.Application.UseCases;

public sealed class CancelPaymentUseCase(
    IPaymentRepository repository,
    IConsentValidator consentValidator,
    ILogger<CancelPaymentUseCase> logger)
{
    public async Task<Result> ExecuteAsync(Guid consentId, Guid paymentId, string reason, CancellationToken ct = default)
    {
        var validation = await consentValidator.ValidateAsync(
            consentId, [OpenFinance.Shared.Consent.OpenFinancePermissions.PaymentsRead], ct);
        if (!validation.IsValid)
        {
            logger.LogWarning("AUDIT: Payment cancellation denied - ConsentId={ConsentId}, PaymentId={PaymentId}: {Reason}",
                consentId, paymentId, validation.ErrorMessage);
            return Result.Failure(validation.ErrorMessage!);
        }

        var payment = await repository.GetByIdAsync(paymentId, ct);
        if (payment is null)
            return Result.Failure($"Payment {paymentId} not found.");

        // Ownership check: payment must belong to the referenced consent
        if (payment.ConsentId != consentId)
        {
            logger.LogWarning("AUDIT: Payment cancellation denied - consent mismatch. ConsentId={ConsentId}, PaymentId={PaymentId}, PaymentConsentId={PaymentConsentId}",
                consentId, paymentId, payment.ConsentId);
            return Result.Failure("CONSENT_MISMATCH: Payment does not belong to the provided consent.");
        }

        try
        {
            payment.Cancel(reason);
            await repository.UpdateAsync(payment, ct);

            logger.LogInformation(
                "AUDIT: Payment cancelled - PaymentId={PaymentId}, ConsentId={ConsentId}, Reason={Reason}",
                paymentId, consentId, reason);

            return Result.Success();
        }
        catch (InvalidOperationException ex)
        {
            logger.LogWarning("AUDIT: Payment cancellation failed - PaymentId={PaymentId}, Error={Error}",
                paymentId, ex.Message);
            return Result.Failure(ex.Message);
        }
    }
}
