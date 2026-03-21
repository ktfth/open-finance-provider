using OpenFinance.ExchangeService.Domain.Entities;

namespace OpenFinance.ExchangeService.Domain.Repositories;

public interface IExchangeRepository
{
    Task<IReadOnlyList<ExchangeOperation>> GetByUserIdAsync(string userId, CancellationToken ct = default);
    Task<ExchangeOperation?> GetByIdAsync(Guid operationId, CancellationToken ct = default);
    Task<IReadOnlyList<ExchangeEvent>> GetEventsAsync(Guid operationId, CancellationToken ct = default);
    Task AddAsync(ExchangeOperation operation, CancellationToken ct = default);
}
