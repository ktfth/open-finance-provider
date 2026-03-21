using OpenFinance.LoanService.Domain.Repositories;
using OpenFinance.Shared.Consent;
using OpenFinance.Shared.Contracts;
using OpenFinance.Shared.Results;

namespace OpenFinance.LoanService.Application.UseCases;

public sealed class GetLoanWarrantiesUseCase(ILoanRepository repository, IConsentValidator consentValidator)
{
    public async Task<Result<LoanWarrantyListResponse>> ExecuteAsync(
        Guid contractId,
        Guid consentId,
        CancellationToken ct = default)
    {
        var validation = await consentValidator.ValidateAsync(
            consentId, [OpenFinancePermissions.LoansWarrantiesRead], ct);
        if (!validation.IsValid)
            return Result.Failure<LoanWarrantyListResponse>(validation.ErrorMessage!);

        var warranties = await repository.GetLoanWarrantiesAsync(contractId, ct);
        var summaries = warranties
            .Select(w => new LoanWarrantySummary(
                w.Id,
                w.Type,
                w.WarrantySubType,
                w.Currency,
                w.Amount))
            .ToList();

        return Result.Success(new LoanWarrantyListResponse(summaries));
    }
}
