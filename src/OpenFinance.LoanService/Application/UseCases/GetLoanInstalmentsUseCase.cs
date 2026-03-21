using OpenFinance.LoanService.Domain.Repositories;
using OpenFinance.Shared.Consent;
using OpenFinance.Shared.Contracts;
using OpenFinance.Shared.Results;

namespace OpenFinance.LoanService.Application.UseCases;

public sealed class GetLoanInstalmentsUseCase(ILoanRepository repository, IConsentValidator consentValidator)
{
    public async Task<Result<LoanInstalmentListResponse>> ExecuteAsync(
        Guid contractId,
        Guid consentId,
        CancellationToken ct = default)
    {
        var validation = await consentValidator.ValidateAsync(
            consentId, [OpenFinancePermissions.LoansScheduledInstalmentsRead], ct);
        if (!validation.IsValid)
            return Result.Failure<LoanInstalmentListResponse>(validation.ErrorMessage!);

        var instalments = await repository.GetLoanInstalmentsAsync(contractId, ct);
        var summaries = instalments
            .Select(i => new LoanInstalmentSummary(
                i.InstalmentNumber,
                i.DueDate,
                i.TotalAmount,
                i.PrincipalAmount,
                i.InterestAmount,
                i.FeesAmount,
                i.Currency,
                i.Status))
            .ToList();

        return Result.Success(new LoanInstalmentListResponse(summaries));
    }
}
