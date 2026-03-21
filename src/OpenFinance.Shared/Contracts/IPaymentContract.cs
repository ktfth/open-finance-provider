using System.ComponentModel.DataAnnotations;

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
    [Required] Guid ConsentId,
    [Required, StringLength(20, MinimumLength = 1)] string DebtorAccountId,
    [Required, StringLength(20, MinimumLength = 1)] string CreditorAccountId,
    [Required, StringLength(200, MinimumLength = 1)] string CreditorName,
    [Required, RegularExpression(@"^\d{3}\.\d{3}\.\d{3}-\d{2}$|^\d{2}\.\d{3}\.\d{3}/\d{4}-\d{2}$|^\d{11}$|^\d{14}$",
        ErrorMessage = "CreditorCpfCnpj must be a valid CPF or CNPJ.")] string CreditorCpfCnpj,
    [Required, Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than zero.")] decimal Amount,
    [Required, StringLength(3, MinimumLength = 3, ErrorMessage = "Currency must be a 3-letter ISO code.")] string Currency,
    [Required, StringLength(500, MinimumLength = 1)] string Description,
    [Required] PaymentType Type
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
