using OpenFinance.Shared.Contracts;
using OpenFinance.Shared.Domain;

namespace OpenFinance.LoanService.Domain.Entities;

public class FinancingContract : Entity
{
    private FinancingContract() { }

    public static FinancingContract Create(
        string userId,
        string contractNumber,
        FinancingType financingType,
        string productName,
        string companyCnpj,
        decimal contractAmount,
        decimal outstandingBalance,
        decimal interestRate,
        InterestRateType interestRateType,
        RateIndexer indexer,
        string currency,
        DateOnly contractDate,
        DateOnly dueDate,
        int instalmentCount,
        int paidInstalmentCount,
        decimal cet,
        AmortizationType amortizationType)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(userId);
        ArgumentException.ThrowIfNullOrWhiteSpace(contractNumber);
        ArgumentException.ThrowIfNullOrWhiteSpace(productName);
        ArgumentException.ThrowIfNullOrWhiteSpace(companyCnpj);
        ArgumentException.ThrowIfNullOrWhiteSpace(currency);

        return new FinancingContract
        {
            UserId = userId,
            ContractNumber = contractNumber,
            FinancingType = financingType,
            ProductName = productName,
            CompanyCnpj = companyCnpj,
            Status = ContractStatus.Active,
            ContractAmount = contractAmount,
            OutstandingBalance = outstandingBalance,
            InterestRate = interestRate,
            InterestRateType = interestRateType,
            Indexer = indexer,
            Currency = currency,
            ContractDate = contractDate,
            DueDate = dueDate,
            InstalmentCount = instalmentCount,
            PaidInstalmentCount = paidInstalmentCount,
            CET = cet,
            AmortizationType = amortizationType
        };
    }

    public string UserId { get; private set; } = default!;
    public string ContractNumber { get; private set; } = default!;
    public FinancingType FinancingType { get; private set; }
    public string ProductName { get; private set; } = default!;
    public string CompanyCnpj { get; private set; } = default!;
    public ContractStatus Status { get; private set; }
    public decimal ContractAmount { get; private set; }
    public decimal OutstandingBalance { get; private set; }
    public decimal InterestRate { get; private set; }
    public InterestRateType InterestRateType { get; private set; }
    public RateIndexer Indexer { get; private set; }
    public string Currency { get; private set; } = default!;
    public DateOnly ContractDate { get; private set; }
    public DateOnly DueDate { get; private set; }
    public int InstalmentCount { get; private set; }
    public int PaidInstalmentCount { get; private set; }
    public decimal CET { get; private set; }
    public AmortizationType AmortizationType { get; private set; }

    public void Settle()
    {
        Status = ContractStatus.Settled;
        OutstandingBalance = 0m;
        SetUpdated();
    }

    public void WriteOff()
    {
        Status = ContractStatus.WrittenOff;
        SetUpdated();
    }
}
