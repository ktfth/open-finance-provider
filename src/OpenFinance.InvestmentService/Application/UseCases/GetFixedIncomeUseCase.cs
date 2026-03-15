using OpenFinance.InvestmentService.Domain.Repositories;
using OpenFinance.Shared.Contracts;
using OpenFinance.Shared.Results;

namespace OpenFinance.InvestmentService.Application.UseCases;

public sealed class GetFixedIncomeUseCase(IInvestmentRepository repository)
{
    public async Task<Result<FixedIncomeListResponse>> ExecuteAsync(
        string userId, Guid consentId, CancellationToken ct = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(userId);

        var investments = await repository.GetFixedIncomeByUserIdAsync(userId, ct);
        var summaries = investments.Select(i => new FixedIncomeSummary(
            i.Id,
            i.Type,
            i.ProductName,
            i.Issuer,
            i.MaturityDate,
            i.GrossAmount,
            i.Currency
        )).ToList();

        return Result.Success(new FixedIncomeListResponse(summaries));
    }
}
