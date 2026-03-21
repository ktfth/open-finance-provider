using OpenFinance.Shared.Contracts;
using OpenFinance.Shared.Domain;

namespace OpenFinance.CapitalizationService.Domain.Entities;

/// <summary>
/// Represents a capitalization bond (título de capitalização).
/// Aligned with the Open Finance Brasil Capitalization API specification (Phase 4).
/// </summary>
public class CapitalizationBond : Entity
{
    private CapitalizationBond() { }

    public static CapitalizationBond Create(
        string userId,
        string bondNumber,
        CapitalizationModality modality,
        string productName,
        string companyName,
        string companyCnpj,
        DateOnly contractDate,
        DateOnly maturityDate,
        int paymentCount,
        decimal paymentAmount,
        PaymentFrequency paymentFrequency,
        decimal latePaymentFine,
        decimal latePaymentInterest,
        decimal redemptionPercentage,
        decimal currentRedemptionValue,
        decimal prizeDrawAmount,
        string currency,
        decimal totalPaidAmount,
        decimal mathematicalReserve,
        decimal surrenderQuota)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(userId);
        ArgumentException.ThrowIfNullOrWhiteSpace(bondNumber);
        ArgumentException.ThrowIfNullOrWhiteSpace(productName);
        ArgumentException.ThrowIfNullOrWhiteSpace(companyName);
        ArgumentException.ThrowIfNullOrWhiteSpace(companyCnpj);
        ArgumentException.ThrowIfNullOrWhiteSpace(currency);

        if (paymentCount <= 0)
            throw new ArgumentException("Payment count must be positive.", nameof(paymentCount));
        if (paymentAmount <= 0)
            throw new ArgumentException("Payment amount must be positive.", nameof(paymentAmount));
        if (maturityDate <= contractDate)
            throw new ArgumentException("Maturity date must be after contract date.", nameof(maturityDate));

        return new CapitalizationBond
        {
            UserId = userId,
            BondNumber = bondNumber,
            Modality = modality,
            ProductName = productName,
            CompanyName = companyName,
            CompanyCnpj = companyCnpj,
            Status = CapitalizationBondStatus.Active,
            ContractDate = contractDate,
            MaturityDate = maturityDate,
            PaymentCount = paymentCount,
            PaymentAmount = paymentAmount,
            PaymentFrequency = paymentFrequency,
            LatePaymentFine = latePaymentFine,
            LatePaymentInterest = latePaymentInterest,
            RedemptionPercentage = redemptionPercentage,
            CurrentRedemptionValue = currentRedemptionValue,
            PrizeDrawAmount = prizeDrawAmount,
            Currency = currency,
            TotalPaidAmount = totalPaidAmount,
            MathematicalReserve = mathematicalReserve,
            SurrenderQuota = surrenderQuota
        };
    }

    public string UserId { get; private set; } = default!;
    public string BondNumber { get; private set; } = default!;
    public CapitalizationModality Modality { get; private set; }
    public string ProductName { get; private set; } = default!;
    public string CompanyName { get; private set; } = default!;
    public string CompanyCnpj { get; private set; } = default!;
    public CapitalizationBondStatus Status { get; private set; }
    public DateOnly ContractDate { get; private set; }
    public DateOnly MaturityDate { get; private set; }
    public int PaymentCount { get; private set; }
    public decimal PaymentAmount { get; private set; }
    public PaymentFrequency PaymentFrequency { get; private set; }
    public decimal LatePaymentFine { get; private set; }
    public decimal LatePaymentInterest { get; private set; }
    public decimal RedemptionPercentage { get; private set; }
    public decimal CurrentRedemptionValue { get; private set; }
    public decimal PrizeDrawAmount { get; private set; }
    public string Currency { get; private set; } = default!;
    public decimal TotalPaidAmount { get; private set; }
    public decimal MathematicalReserve { get; private set; }
    public decimal SurrenderQuota { get; private set; }

    public void Redeem()
    {
        if (Status != CapitalizationBondStatus.Active && Status != CapitalizationBondStatus.Suspended)
            throw new InvalidOperationException($"Cannot redeem a bond with status {Status}.");

        Status = CapitalizationBondStatus.Redeemed;
        SetUpdated();
    }

    public void Suspend()
    {
        if (Status != CapitalizationBondStatus.Active)
            throw new InvalidOperationException($"Cannot suspend a bond with status {Status}.");

        Status = CapitalizationBondStatus.Suspended;
        SetUpdated();
    }

    public void Expire()
    {
        if (Status != CapitalizationBondStatus.Active && Status != CapitalizationBondStatus.Suspended)
            throw new InvalidOperationException($"Cannot expire a bond with status {Status}.");

        Status = CapitalizationBondStatus.Expired;
        SetUpdated();
    }
}
