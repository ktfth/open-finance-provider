using OpenFinance.Shared.Contracts;
using OpenFinance.Shared.Domain;

namespace OpenFinance.LoanService.Domain.Entities;

public class OverdraftContract : Entity
{
    private OverdraftContract() { }

    public static OverdraftContract Create(
        string userId,
        string contractNumber,
        string companyCnpj,
        decimal contractAmount,
        decimal outstandingBalance,
        decimal interestRate,
        InterestRateType interestRateType,
        RateIndexer indexer,
        string currency,
        DateOnly contractDate)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(userId);
        ArgumentException.ThrowIfNullOrWhiteSpace(contractNumber);
        ArgumentException.ThrowIfNullOrWhiteSpace(companyCnpj);
        ArgumentException.ThrowIfNullOrWhiteSpace(currency);

        return new OverdraftContract
        {
            UserId = userId,
            ContractNumber = contractNumber,
            CompanyCnpj = companyCnpj,
            Status = ContractStatus.Active,
            ContractAmount = contractAmount,
            OutstandingBalance = outstandingBalance,
            InterestRate = interestRate,
            InterestRateType = interestRateType,
            Indexer = indexer,
            Currency = currency,
            ContractDate = contractDate
        };
    }

    public string UserId { get; private set; } = default!;
    public string ContractNumber { get; private set; } = default!;
    public string CompanyCnpj { get; private set; } = default!;
    public ContractStatus Status { get; private set; }
    public decimal ContractAmount { get; private set; }
    public decimal OutstandingBalance { get; private set; }
    public decimal InterestRate { get; private set; }
    public InterestRateType InterestRateType { get; private set; }
    public RateIndexer Indexer { get; private set; }
    public string Currency { get; private set; } = default!;
    public DateOnly ContractDate { get; private set; }
}
