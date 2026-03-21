using OpenFinance.Shared.Contracts;
using OpenFinance.Shared.Domain;

namespace OpenFinance.LoanService.Domain.Entities;

public class LoanInstalment : Entity
{
    private LoanInstalment() { }

    public static LoanInstalment Create(
        Guid contractId,
        int instalmentNumber,
        DateOnly dueDate,
        decimal totalAmount,
        decimal principalAmount,
        decimal interestAmount,
        decimal feesAmount,
        string currency,
        InstalmentStatus status)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(currency);

        return new LoanInstalment
        {
            ContractId = contractId,
            InstalmentNumber = instalmentNumber,
            DueDate = dueDate,
            TotalAmount = totalAmount,
            PrincipalAmount = principalAmount,
            InterestAmount = interestAmount,
            FeesAmount = feesAmount,
            Currency = currency,
            Status = status
        };
    }

    public Guid ContractId { get; private set; }
    public int InstalmentNumber { get; private set; }
    public DateOnly DueDate { get; private set; }
    public decimal TotalAmount { get; private set; }
    public decimal PrincipalAmount { get; private set; }
    public decimal InterestAmount { get; private set; }
    public decimal FeesAmount { get; private set; }
    public string Currency { get; private set; } = default!;
    public InstalmentStatus Status { get; private set; }
}
