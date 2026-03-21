using OpenFinance.InsuranceService.Domain.Repositories;
using OpenFinance.Shared.Consent;
using OpenFinance.Shared.Contracts;
using OpenFinance.Shared.Results;

namespace OpenFinance.InsuranceService.Application.UseCases;

/// <summary>
/// Returns premium payment details for a specific insurance policy,
/// including aggregated totals and individual payment installments.
/// Requires permission: INSURANCE_PREMIUM_READ
/// </summary>
public sealed class GetInsurancePremiumUseCase(IInsuranceRepository repository, IConsentValidator consentValidator)
{
    public async Task<Result<InsurancePremiumResponse?>> ExecuteAsync(
        Guid insuranceId,
        Guid consentId,
        CancellationToken ct = default)
    {
        var validation = await consentValidator.ValidateAsync(
            consentId, [OpenFinancePermissions.InsurancesRead], ct);
        if (!validation.IsValid)
            return Result.Failure<InsurancePremiumResponse?>(validation.ErrorMessage!);

        var insurance = await repository.GetByIdAsync(insuranceId, ct);
        if (insurance is null)
            return Result.Success<InsurancePremiumResponse?>(null);

        var payments = await repository.GetPremiumPaymentsAsync(insuranceId, ct);

        var totalPremium = payments.Sum(p => p.Amount);
        var paidAmount = payments
            .Where(p => p.Status == PremiumPaymentStatus.Paid)
            .Sum(p => p.Amount);
        var outstandingAmount = payments
            .Where(p => p.Status is PremiumPaymentStatus.Pending or PremiumPaymentStatus.Overdue)
            .Sum(p => p.Amount);

        var nextDue = payments
            .Where(p => p.Status == PremiumPaymentStatus.Pending)
            .OrderBy(p => p.DueDate)
            .Select(p => p.DueDate)
            .FirstOrDefault(DateOnly.FromDateTime(DateTime.UtcNow.AddMonths(1)));

        var paymentSummaries = payments
            .OrderBy(p => p.DueDate)
            .Select(p => new InsurancePremiumPayment(
                p.Id,
                p.DueDate,
                p.Amount,
                p.Currency,
                p.Status))
            .ToList();

        var response = new InsurancePremiumResponse(
            insurance.Id,
            totalPremium,
            paidAmount,
            outstandingAmount,
            insurance.Currency,
            PaymentFrequency.Monthly,
            nextDue,
            paymentSummaries);

        return Result.Success<InsurancePremiumResponse?>(response);
    }
}
