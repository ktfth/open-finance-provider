using OpenFinance.LoanService.Domain.Repositories;
using OpenFinance.Shared.Consent;
using OpenFinance.Shared.Contracts;
using OpenFinance.Shared.Results;

namespace OpenFinance.LoanService.Application.UseCases;

public sealed class GetLoanPaymentsUseCase(ILoanRepository repository, IConsentValidator consentValidator)
{
    public async Task<Result<LoanPaymentListResponse>> ExecuteAsync(
        Guid contractId,
        Guid consentId,
        CancellationToken ct = default)
    {
        var validation = await consentValidator.ValidateAsync(
            consentId, [OpenFinancePermissions.LoansPaymentsRead], ct);
        if (!validation.IsValid)
            return Result.Failure<LoanPaymentListResponse>(validation.ErrorMessage!);

        var payments = await repository.GetLoanPaymentsAsync(contractId, ct);
        var summaries = payments
            .Select(p => new LoanPaymentSummary(
                p.Id,
                p.PaymentDate,
                p.PaidAmount,
                p.PrincipalAmount,
                p.InterestAmount,
                p.FeesAmount,
                p.ChargesAmount,
                p.Currency,
                p.IsOverdue))
            .ToList();

        return Result.Success(new LoanPaymentListResponse(summaries));
    }
}
