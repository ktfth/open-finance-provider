using OpenFinance.LoanService.Domain.Repositories;
using OpenFinance.Shared.Consent;
using OpenFinance.Shared.Contracts;
using OpenFinance.Shared.Results;

namespace OpenFinance.LoanService.Application.UseCases;

public sealed class GetLoanDetailsUseCase(ILoanRepository repository, IConsentValidator consentValidator)
{
    public async Task<Result<LoanDetailsResponse?>> ExecuteAsync(
        Guid contractId,
        Guid consentId,
        CancellationToken ct = default)
    {
        var validation = await consentValidator.ValidateAsync(
            consentId, [OpenFinancePermissions.LoansRead], ct);
        if (!validation.IsValid)
            return Result.Failure<LoanDetailsResponse?>(validation.ErrorMessage!);

        var loan = await repository.GetLoanByIdAsync(contractId, ct);
        if (loan is null)
            return Result.Success<LoanDetailsResponse?>(null);

        return Result.Success<LoanDetailsResponse?>(new LoanDetailsResponse(
            loan.Id,
            loan.ContractNumber,
            loan.Type,
            loan.ProductName,
            loan.CompanyCnpj,
            loan.Status,
            loan.ContractAmount,
            loan.OutstandingBalance,
            loan.InterestRate,
            loan.InterestRateType,
            loan.Indexer,
            loan.Currency,
            loan.ContractDate,
            loan.DueDate,
            loan.SettlementDate,
            loan.InstalmentCount,
            loan.PaidInstalmentCount,
            loan.CET,
            loan.AmortizationType,
            Array.Empty<ContractFee>()));
    }
}
