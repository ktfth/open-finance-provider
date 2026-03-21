using OpenFinance.Shared.Domain;

namespace OpenFinance.LoanService.Domain.Entities;

public class LoanPayment : Entity
{
    private LoanPayment() { }

    public static LoanPayment Create(
        Guid contractId,
        DateOnly paymentDate,
        decimal paidAmount,
        decimal principalAmount,
        decimal interestAmount,
        decimal feesAmount,
        decimal chargesAmount,
        string currency,
        bool isOverdue)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(currency);

        return new LoanPayment
        {
            ContractId = contractId,
            PaymentDate = paymentDate,
            PaidAmount = paidAmount,
            PrincipalAmount = principalAmount,
            InterestAmount = interestAmount,
            FeesAmount = feesAmount,
            ChargesAmount = chargesAmount,
            Currency = currency,
            IsOverdue = isOverdue
        };
    }

    public Guid ContractId { get; private set; }
    public DateOnly PaymentDate { get; private set; }
    public decimal PaidAmount { get; private set; }
    public decimal PrincipalAmount { get; private set; }
    public decimal InterestAmount { get; private set; }
    public decimal FeesAmount { get; private set; }
    public decimal ChargesAmount { get; private set; }
    public string Currency { get; private set; } = default!;
    public bool IsOverdue { get; private set; }
}
