using OpenFinance.LoanService.Domain.Repositories;
using OpenFinance.Shared.Consent;
using OpenFinance.Shared.Contracts;
using OpenFinance.Shared.Results;

namespace OpenFinance.LoanService.Application.UseCases;

public sealed class GetLoansUseCase(ILoanRepository repository, IConsentValidator consentValidator)
{
    public async Task<Result<LoanListResponse>> ExecuteAsync(
        string userId,
        Guid consentId,
        CancellationToken ct = default)
    {
        var validation = await consentValidator.ValidateAsync(
            consentId, [OpenFinancePermissions.LoansRead], ct);
        if (!validation.IsValid)
            return Result.Failure<LoanListResponse>(validation.ErrorMessage!);

        var loans = await repository.GetLoansByUserIdAsync(userId, ct);
        var summaries = loans
            .Select(l => new LoanSummary(
                l.Id,
                l.ContractNumber,
                l.Type,
                l.ProductName,
                l.Status,
                l.ContractAmount,
                l.Currency,
                l.ContractDate,
                l.DueDate))
            .ToList();

        return Result.Success(new LoanListResponse(summaries));
    }
}
