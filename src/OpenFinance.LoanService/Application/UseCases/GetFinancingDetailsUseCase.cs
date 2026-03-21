using OpenFinance.LoanService.Domain.Repositories;
using OpenFinance.Shared.Consent;
using OpenFinance.Shared.Contracts;
using OpenFinance.Shared.Results;

namespace OpenFinance.LoanService.Application.UseCases;

public sealed class GetFinancingDetailsUseCase(ILoanRepository repository, IConsentValidator consentValidator)
{
    public async Task<Result<FinancingDetailsResponse?>> ExecuteAsync(
        Guid contractId,
        Guid consentId,
        CancellationToken ct = default)
    {
        var validation = await consentValidator.ValidateAsync(
            consentId, [OpenFinancePermissions.FinancingsRead], ct);
        if (!validation.IsValid)
            return Result.Failure<FinancingDetailsResponse?>(validation.ErrorMessage!);

        var financing = await repository.GetFinancingByIdAsync(contractId, ct);
        if (financing is null)
            return Result.Success<FinancingDetailsResponse?>(null);

        return Result.Success<FinancingDetailsResponse?>(new FinancingDetailsResponse(
            financing.Id,
            financing.ContractNumber,
            financing.FinancingType,
            financing.ProductName,
            financing.CompanyCnpj,
            financing.Status,
            financing.ContractAmount,
            financing.OutstandingBalance,
            financing.InterestRate,
            financing.InterestRateType,
            financing.Indexer,
            financing.Currency,
            financing.ContractDate,
            financing.DueDate,
            financing.InstalmentCount,
            financing.PaidInstalmentCount,
            financing.CET,
            financing.AmortizationType));
    }
}
