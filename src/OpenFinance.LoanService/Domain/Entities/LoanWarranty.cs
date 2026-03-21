using OpenFinance.Shared.Contracts;
using OpenFinance.Shared.Domain;

namespace OpenFinance.LoanService.Domain.Entities;

public class LoanWarranty : Entity
{
    private LoanWarranty() { }

    public static LoanWarranty Create(
        Guid contractId,
        WarrantyType type,
        string warrantySubType,
        string currency,
        decimal amount)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(warrantySubType);
        ArgumentException.ThrowIfNullOrWhiteSpace(currency);

        return new LoanWarranty
        {
            ContractId = contractId,
            Type = type,
            WarrantySubType = warrantySubType,
            Currency = currency,
            Amount = amount
        };
    }

    public Guid ContractId { get; private set; }
    public WarrantyType Type { get; private set; }
    public string WarrantySubType { get; private set; } = default!;
    public string Currency { get; private set; } = default!;
    public decimal Amount { get; private set; }
}
