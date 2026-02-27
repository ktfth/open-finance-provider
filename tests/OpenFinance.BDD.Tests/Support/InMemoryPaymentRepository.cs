using OpenFinance.PaymentService.Domain.Entities;
using OpenFinance.PaymentService.Domain.Repositories;

namespace OpenFinance.BDD.Tests.Support;

public class InMemoryPaymentRepository : IPaymentRepository
{
    private readonly Dictionary<Guid, Payment> _store = [];

    public Task<Payment?> GetByIdAsync(Guid paymentId, CancellationToken ct = default) =>
        Task.FromResult(_store.GetValueOrDefault(paymentId));

    public Task<IReadOnlyList<Payment>> GetByConsentIdAsync(Guid consentId, CancellationToken ct = default) =>
        Task.FromResult<IReadOnlyList<Payment>>(
            _store.Values.Where(p => p.ConsentId == consentId).ToList());

    public Task AddAsync(Payment payment, CancellationToken ct = default)
    {
        _store[payment.Id] = payment;
        return Task.CompletedTask;
    }

    public Task UpdateAsync(Payment payment, CancellationToken ct = default)
    {
        _store[payment.Id] = payment;
        return Task.CompletedTask;
    }
}
