namespace OpenFinance.Shared.Contracts;

/// <summary>
/// Defines the contract for payment initiation services provided to bank participants.
/// </summary>
public interface IPaymentContract
{
    Task<PaymentResponse> InitiatePaymentAsync(InitiatePaymentRequest request, CancellationToken ct = default);
    Task<PaymentResponse?> GetPaymentStatusAsync(Guid paymentId, CancellationToken ct = default);
    Task<bool> CancelPaymentAsync(Guid paymentId, string reason, CancellationToken ct = default);
}

public record InitiatePaymentRequest(
    Guid ConsentId,
    string DebtorAccountId,
    string CreditorAccountId,
    string CreditorName,
    string CreditorCpfCnpj,
    decimal Amount,
    string Currency,
    string Description,
    PaymentType Type
);

public record PaymentResponse(
    Guid PaymentId,
    Guid ConsentId,
    PaymentStatus Status,
    decimal Amount,
    string Currency,
    string Description,
    DateTime CreatedAt,
    DateTime? CompletedAt
);

public enum PaymentStatus
{
    Pending,
    Processing,
    Completed,
    Failed,
    Cancelled
}

public enum PaymentType
{
    Pix,
    Ted,
    Doc
}
