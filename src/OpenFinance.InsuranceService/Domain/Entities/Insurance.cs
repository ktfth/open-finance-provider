using OpenFinance.Shared.Contracts;
using OpenFinance.Shared.Domain;

namespace OpenFinance.InsuranceService.Domain.Entities;

/// <summary>
/// Represents an insurance policy held by a customer.
/// Maps to the Open Finance Brasil Insurance resource (Phase 4).
/// </summary>
public class Insurance : Entity
{
    private Insurance() { }

    public static Insurance Create(
        string userId,
        InsuranceType type,
        string productName,
        string insurerName,
        string insurerCnpj,
        string policyNumber,
        DateOnly proposalDate,
        DateOnly effectiveDate,
        DateOnly expirationDate,
        decimal insuredAmount,
        decimal premiumAmount,
        string currency,
        int gracePeriodDays,
        string insuredCpfCnpj,
        string insuredName,
        string beneficiaryName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(userId);
        ArgumentException.ThrowIfNullOrWhiteSpace(productName);
        ArgumentException.ThrowIfNullOrWhiteSpace(insurerName);
        ArgumentException.ThrowIfNullOrWhiteSpace(insurerCnpj);
        ArgumentException.ThrowIfNullOrWhiteSpace(policyNumber);
        ArgumentException.ThrowIfNullOrWhiteSpace(currency);
        ArgumentException.ThrowIfNullOrWhiteSpace(insuredCpfCnpj);
        ArgumentException.ThrowIfNullOrWhiteSpace(insuredName);
        ArgumentException.ThrowIfNullOrWhiteSpace(beneficiaryName);

        if (insuredAmount < 0)
            throw new ArgumentException("Insured amount cannot be negative.", nameof(insuredAmount));
        if (premiumAmount < 0)
            throw new ArgumentException("Premium amount cannot be negative.", nameof(premiumAmount));
        if (gracePeriodDays < 0)
            throw new ArgumentException("Grace period days cannot be negative.", nameof(gracePeriodDays));
        if (expirationDate <= effectiveDate)
            throw new ArgumentException("Expiration date must be after effective date.", nameof(expirationDate));

        return new Insurance
        {
            UserId = userId,
            Type = type,
            ProductName = productName,
            InsurerName = insurerName,
            InsurerCnpj = insurerCnpj,
            Status = InsuranceStatus.PendingActivation,
            PolicyNumber = policyNumber,
            ProposalDate = proposalDate,
            EffectiveDate = effectiveDate,
            ExpirationDate = expirationDate,
            InsuredAmount = insuredAmount,
            PremiumAmount = premiumAmount,
            Currency = currency,
            GracePeriodDays = gracePeriodDays,
            InsuredCpfCnpj = insuredCpfCnpj,
            InsuredName = insuredName,
            BeneficiaryName = beneficiaryName
        };
    }

    public string UserId { get; private set; } = default!;
    public InsuranceType Type { get; private set; }
    public string ProductName { get; private set; } = default!;
    public string InsurerName { get; private set; } = default!;
    public string InsurerCnpj { get; private set; } = default!;
    public InsuranceStatus Status { get; private set; }
    public string PolicyNumber { get; private set; } = default!;
    public DateOnly ProposalDate { get; private set; }
    public DateOnly EffectiveDate { get; private set; }
    public DateOnly ExpirationDate { get; private set; }
    public decimal InsuredAmount { get; private set; }
    public decimal PremiumAmount { get; private set; }
    public string Currency { get; private set; } = default!;
    public int GracePeriodDays { get; private set; }
    public string InsuredCpfCnpj { get; private set; } = default!;
    public string InsuredName { get; private set; } = default!;
    public string BeneficiaryName { get; private set; } = default!;

    public void Activate()
    {
        if (Status == InsuranceStatus.Cancelled)
            throw new InvalidOperationException("Cannot activate a cancelled insurance policy.");
        Status = InsuranceStatus.Active;
        SetUpdated();
    }

    public void Cancel()
    {
        if (Status == InsuranceStatus.Cancelled)
            throw new InvalidOperationException("Insurance policy is already cancelled.");
        Status = InsuranceStatus.Cancelled;
        SetUpdated();
    }

    public void Suspend()
    {
        if (Status != InsuranceStatus.Active)
            throw new InvalidOperationException("Only active insurance policies can be suspended.");
        Status = InsuranceStatus.Suspended;
        SetUpdated();
    }

    public bool IsActive() => Status == InsuranceStatus.Active;
}
