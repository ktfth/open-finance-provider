using OpenFinance.LoanService.Domain.Repositories;
using OpenFinance.Shared.Consent;
using OpenFinance.Shared.Contracts;
using OpenFinance.Shared.Results;

namespace OpenFinance.LoanService.Application.UseCases;

public sealed class GetOverdraftDetailsUseCase(ILoanRepository repository, IConsentValidator consentValidator)
{
    public async Task<Result<OverdraftDetailsResponse?>> ExecuteAsync(
        Guid contractId,
        Guid consentId,
        CancellationToken ct = default)
    {
        var validation = await consentValidator.ValidateAsync(
            consentId, [OpenFinancePermissions.UnarrangedAccountsOverdraftRead], ct);
        if (!validation.IsValid)
            return Result.Failure<OverdraftDetailsResponse?>(validation.ErrorMessage!);

        var overdraft = await repository.GetOverdraftByIdAsync(contractId, ct);
        if (overdraft is null)
            return Result.Success<OverdraftDetailsResponse?>(null);

        return Result.Success<OverdraftDetailsResponse?>(new OverdraftDetailsResponse(
            overdraft.Id,
            overdraft.ContractNumber,
            overdraft.CompanyCnpj,
            overdraft.Status,
            overdraft.ContractAmount,
            overdraft.OutstandingBalance,
            overdraft.InterestRate,
            overdraft.InterestRateType,
            overdraft.Indexer,
            overdraft.Currency,
            overdraft.ContractDate,
            Array.Empty<ContractFee>()));
    }
}
