using OpenFinance.LoanService.Domain.Repositories;
using OpenFinance.Shared.Consent;
using OpenFinance.Shared.Contracts;
using OpenFinance.Shared.Results;

namespace OpenFinance.LoanService.Application.UseCases;

public sealed class GetOverdraftsUseCase(ILoanRepository repository, IConsentValidator consentValidator)
{
    public async Task<Result<OverdraftListResponse>> ExecuteAsync(
        string userId,
        Guid consentId,
        CancellationToken ct = default)
    {
        var validation = await consentValidator.ValidateAsync(
            consentId, [OpenFinancePermissions.UnarrangedAccountsOverdraftRead], ct);
        if (!validation.IsValid)
            return Result.Failure<OverdraftListResponse>(validation.ErrorMessage!);

        var overdrafts = await repository.GetOverdraftsByUserIdAsync(userId, ct);
        var summaries = overdrafts
            .Select(o => new OverdraftSummary(
                o.Id,
                o.ContractNumber,
                o.Status,
                o.ContractAmount,
                o.Currency,
                o.ContractDate))
            .ToList();

        return Result.Success(new OverdraftListResponse(summaries));
    }
}
