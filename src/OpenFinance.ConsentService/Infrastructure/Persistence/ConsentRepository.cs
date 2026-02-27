using Microsoft.EntityFrameworkCore;
using OpenFinance.ConsentService.Domain.Entities;
using OpenFinance.ConsentService.Domain.Repositories;

namespace OpenFinance.ConsentService.Infrastructure.Persistence;

public class ConsentRepository(ConsentDbContext context) : IConsentRepository
{
    public async Task<Consent?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        await context.Consents.FirstOrDefaultAsync(c => c.Id == id, ct);

    public async Task<IReadOnlyList<Consent>> GetByUserIdAsync(string userId, CancellationToken ct = default) =>
        await context.Consents.Where(c => c.UserId == userId).ToListAsync(ct);

    public async Task AddAsync(Consent consent, CancellationToken ct = default)
    {
        await context.Consents.AddAsync(consent, ct);
        await context.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(Consent consent, CancellationToken ct = default)
    {
        context.Consents.Update(consent);
        await context.SaveChangesAsync(ct);
    }

    public async Task<bool> ExistsAsync(Guid id, CancellationToken ct = default) =>
        await context.Consents.AnyAsync(c => c.Id == id, ct);
}
