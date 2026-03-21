using OpenFinance.Shared.Contracts;
using OpenFinance.Shared.Domain;

namespace OpenFinance.InsuranceService.Domain.Entities;

/// <summary>
/// Represents a claim filed against an insurance policy.
/// Maps to the Open Finance Brasil Insurance Claims resource.
/// </summary>
public class InsuranceClaim : Entity
{
    private InsuranceClaim() { }

    public static InsuranceClaim Create(
        Guid insuranceId,
        string claimNumber,
        DateOnly occurrenceDate,
        DateOnly notificationDate,
        decimal claimedAmount,
        string currency)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(claimNumber);
        ArgumentException.ThrowIfNullOrWhiteSpace(currency);

        if (claimedAmount <= 0)
            throw new ArgumentException("Claimed amount must be positive.", nameof(claimedAmount));

        return new InsuranceClaim
        {
            InsuranceId = insuranceId,
            ClaimNumber = claimNumber,
            OccurrenceDate = occurrenceDate,
            NotificationDate = notificationDate,
            ClaimedAmount = claimedAmount,
            ApprovedAmount = null,
            Currency = currency,
            Status = ClaimStatus.Open
        };
    }

    public Guid InsuranceId { get; private set; }
    public string ClaimNumber { get; private set; } = default!;
    public DateOnly OccurrenceDate { get; private set; }
    public DateOnly NotificationDate { get; private set; }
    public decimal ClaimedAmount { get; private set; }
    public decimal? ApprovedAmount { get; private set; }
    public string Currency { get; private set; } = default!;
    public ClaimStatus Status { get; private set; }

    public void Approve(decimal approvedAmount)
    {
        if (approvedAmount < 0)
            throw new ArgumentException("Approved amount cannot be negative.", nameof(approvedAmount));

        ApprovedAmount = approvedAmount;
        Status = approvedAmount >= ClaimedAmount ? ClaimStatus.Approved : ClaimStatus.PartiallyApproved;
        SetUpdated();
    }

    public void Deny()
    {
        Status = ClaimStatus.Denied;
        ApprovedAmount = 0m;
        SetUpdated();
    }

    public void Close()
    {
        Status = ClaimStatus.Closed;
        SetUpdated();
    }
}
