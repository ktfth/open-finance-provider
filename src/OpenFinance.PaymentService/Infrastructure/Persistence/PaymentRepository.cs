using Microsoft.EntityFrameworkCore;
using OpenFinance.PaymentService.Domain.Entities;
using OpenFinance.PaymentService.Domain.Repositories;

namespace OpenFinance.PaymentService.Infrastructure.Persistence;

public class PaymentRepository(PaymentDbContext context) : IPaymentRepository
{
    public async Task<Payment?> GetByIdAsync(Guid paymentId, CancellationToken ct = default) =>
        await context.Payments.FirstOrDefaultAsync(p => p.Id == paymentId, ct);

    public async Task<Payment?> GetByIdempotencyKeyAsync(string idempotencyKey, CancellationToken ct = default) =>
        await context.Payments.FirstOrDefaultAsync(p => p.IdempotencyKey == idempotencyKey, ct);

    public async Task<IReadOnlyList<Payment>> GetByConsentIdAsync(Guid consentId, CancellationToken ct = default) =>
        await context.Payments.Where(p => p.ConsentId == consentId).ToListAsync(ct);

    public async Task AddAsync(Payment payment, CancellationToken ct = default)
    {
        await context.Payments.AddAsync(payment, ct);
        await context.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(Payment payment, CancellationToken ct = default)
    {
        context.Payments.Update(payment);
        await context.SaveChangesAsync(ct);
    }
}
