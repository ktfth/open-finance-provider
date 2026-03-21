using OpenFinance.Shared.Contracts;
using OpenFinance.Shared.Domain;

namespace OpenFinance.PaymentService.Domain.Entities;

public class Payment : Entity
{
    private Payment() { }

    public static Payment Create(
        Guid consentId,
        string debtorAccountId,
        string creditorAccountId,
        string creditorName,
        string creditorCpfCnpj,
        decimal amount,
        string currency,
        string description,
        PaymentType type,
        string? idempotencyKey = null)
    {
        if (amount <= 0)
            throw new ArgumentException("Amount must be positive.", nameof(amount));
        ArgumentException.ThrowIfNullOrWhiteSpace(debtorAccountId);
        ArgumentException.ThrowIfNullOrWhiteSpace(creditorAccountId);
        ArgumentException.ThrowIfNullOrWhiteSpace(creditorName);

        return new Payment
        {
            ConsentId = consentId,
            DebtorAccountId = debtorAccountId,
            CreditorAccountId = creditorAccountId,
            CreditorName = creditorName,
            CreditorCpfCnpj = creditorCpfCnpj,
            Amount = amount,
            Currency = currency,
            Description = description,
            Type = type,
            Status = PaymentStatus.Pending,
            IdempotencyKey = idempotencyKey
        };
    }

    public Guid ConsentId { get; private set; }
    public string DebtorAccountId { get; private set; } = default!;
    public string CreditorAccountId { get; private set; } = default!;
    public string CreditorName { get; private set; } = default!;
    public string CreditorCpfCnpj { get; private set; } = default!;
    public decimal Amount { get; private set; }
    public string Currency { get; private set; } = default!;
    public string Description { get; private set; } = string.Empty;
    public PaymentType Type { get; private set; }
    public PaymentStatus Status { get; private set; }
    public DateTime? CompletedAt { get; private set; }
    public string? FailureReason { get; private set; }
    public string? IdempotencyKey { get; private set; }

    public void MarkProcessing()
    {
        if (Status != PaymentStatus.Pending)
            throw new InvalidOperationException($"Cannot process payment in status {Status}.");
        Status = PaymentStatus.Processing;
        SetUpdated();
    }

    public void Complete()
    {
        if (Status != PaymentStatus.Processing)
            throw new InvalidOperationException($"Cannot complete payment in status {Status}.");
        Status = PaymentStatus.Completed;
        CompletedAt = DateTime.UtcNow;
        SetUpdated();
    }

    public void Fail(string reason)
    {
        if (Status is PaymentStatus.Completed or PaymentStatus.Cancelled)
            throw new InvalidOperationException($"Cannot fail payment in status {Status}.");
        Status = PaymentStatus.Failed;
        FailureReason = reason;
        SetUpdated();
    }

    public void Cancel(string reason)
    {
        if (Status is PaymentStatus.Completed or PaymentStatus.Failed)
            throw new InvalidOperationException($"Cannot cancel payment in status {Status}.");
        Status = PaymentStatus.Cancelled;
        FailureReason = reason;
        SetUpdated();
    }
}
