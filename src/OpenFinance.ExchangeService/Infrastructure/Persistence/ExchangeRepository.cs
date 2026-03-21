using Microsoft.EntityFrameworkCore;
using OpenFinance.ExchangeService.Domain.Entities;
using OpenFinance.ExchangeService.Domain.Repositories;

namespace OpenFinance.ExchangeService.Infrastructure.Persistence;

public class ExchangeRepository(ExchangeDbContext context) : IExchangeRepository
{
    public async Task<IReadOnlyList<ExchangeOperation>> GetByUserIdAsync(
        string userId, CancellationToken ct = default) =>
        await context.ExchangeOperations
            .Where(o => o.UserId == userId)
            .OrderByDescending(o => o.OperationDate)
            .ToListAsync(ct);

    public async Task<ExchangeOperation?> GetByIdAsync(
        Guid operationId, CancellationToken ct = default) =>
        await context.ExchangeOperations
            .FirstOrDefaultAsync(o => o.Id == operationId, ct);

    public async Task<IReadOnlyList<ExchangeEvent>> GetEventsAsync(
        Guid operationId, CancellationToken ct = default) =>
        await context.ExchangeEvents
            .Where(e => e.OperationId == operationId)
            .OrderByDescending(e => e.EventDate)
            .ToListAsync(ct);

    public async Task AddAsync(ExchangeOperation operation, CancellationToken ct = default)
    {
        await context.ExchangeOperations.AddAsync(operation, ct);
        await context.SaveChangesAsync(ct);
    }
}
