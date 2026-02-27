using OpenFinance.PaymentService.Domain.Entities;

namespace OpenFinance.PaymentService.Domain.Repositories;

public interface IPaymentRepository
{
    Task<Payment?> GetByIdAsync(Guid paymentId, CancellationToken ct = default);
    Task<IReadOnlyList<Payment>> GetByConsentIdAsync(Guid consentId, CancellationToken ct = default);
    Task AddAsync(Payment payment, CancellationToken ct = default);
    Task UpdateAsync(Payment payment, CancellationToken ct = default);
}
