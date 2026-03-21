using OpenFinance.LoanService.Domain.Repositories;
using OpenFinance.Shared.Consent;
using OpenFinance.Shared.Contracts;
using OpenFinance.Shared.Results;

namespace OpenFinance.LoanService.Application.UseCases;

public sealed class GetFinancingsUseCase(ILoanRepository repository, IConsentValidator consentValidator)
{
    public async Task<Result<FinancingListResponse>> ExecuteAsync(
        string userId,
        Guid consentId,
        CancellationToken ct = default)
    {
        var validation = await consentValidator.ValidateAsync(
            consentId, [OpenFinancePermissions.FinancingsRead], ct);
        if (!validation.IsValid)
            return Result.Failure<FinancingListResponse>(validation.ErrorMessage!);

        var financings = await repository.GetFinancingsByUserIdAsync(userId, ct);
        var summaries = financings
            .Select(f => new FinancingSummary(
                f.Id,
                f.ContractNumber,
                f.FinancingType,
                f.ProductName,
                f.Status,
                f.ContractAmount,
                f.Currency,
                f.ContractDate,
                f.DueDate))
            .ToList();

        return Result.Success(new FinancingListResponse(summaries));
    }
}
