using Microsoft.Extensions.Logging;
using OpenFinance.PaymentService.Domain.Entities;
using OpenFinance.PaymentService.Domain.Repositories;
using OpenFinance.Shared.Consent;
using OpenFinance.Shared.Contracts;
using OpenFinance.Shared.Results;

namespace OpenFinance.PaymentService.Application.UseCases;

public sealed class InitiatePaymentUseCase(
    IPaymentRepository repository,
    IConsentValidator consentValidator,
    ILogger<InitiatePaymentUseCase> logger)
{
    public async Task<Result<PaymentResponse>> ExecuteAsync(
        InitiatePaymentRequest request,
        string? idempotencyKey = null,
        CancellationToken ct = default)
    {
        // Idempotency check: return existing payment if key was already used
        if (!string.IsNullOrWhiteSpace(idempotencyKey))
        {
            var existing = await repository.GetByIdempotencyKeyAsync(idempotencyKey, ct);
            if (existing is not null)
            {
                logger.LogInformation(
                    "AUDIT: Idempotent payment request - IdempotencyKey={IdempotencyKey}, existing PaymentId={PaymentId}",
                    idempotencyKey, existing.Id);
                return Result.Success(MapToResponse(existing));
            }
        }

        var validation = await consentValidator.ValidateAsync(
            request.ConsentId, [OpenFinance.Shared.Consent.OpenFinancePermissions.PaymentsInitiate], ct);
        if (!validation.IsValid)
        {
            logger.LogWarning("AUDIT: Payment initiation denied - consent {ConsentId} invalid: {Reason}",
                request.ConsentId, validation.ErrorMessage);
            return Result.Failure<PaymentResponse>(validation.ErrorMessage!);
        }

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
                request.Type,
                idempotencyKey);

            await repository.AddAsync(payment, ct);

            logger.LogInformation(
                "AUDIT: Payment initiated - PaymentId={PaymentId}, ConsentId={ConsentId}, Amount={Amount} {Currency}, Type={Type}, Creditor={CreditorCpfCnpj}, IdempotencyKey={IdempotencyKey}",
                payment.Id, request.ConsentId, request.Amount, request.Currency, request.Type, request.CreditorCpfCnpj, idempotencyKey);

            return Result.Success(MapToResponse(payment));
        }
        catch (ArgumentException ex)
        {
            logger.LogWarning("AUDIT: Payment initiation failed - ConsentId={ConsentId}, Error={Error}",
                request.ConsentId, ex.Message);
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
