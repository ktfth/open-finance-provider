using OpenFinance.ConsentService.Domain.Entities;
using OpenFinance.ConsentService.Domain.Repositories;

namespace OpenFinance.BDD.Tests.Support;

public class InMemoryConsentRepository : IConsentRepository
{
    private readonly Dictionary<Guid, Consent> _store = [];

    public Task<Consent?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        Task.FromResult(_store.GetValueOrDefault(id));

    public Task<IReadOnlyList<Consent>> GetByUserIdAsync(string userId, CancellationToken ct = default) =>
        Task.FromResult<IReadOnlyList<Consent>>(
            _store.Values.Where(c => c.UserId == userId).ToList());

    public Task AddAsync(Consent consent, CancellationToken ct = default)
    {
        _store[consent.Id] = consent;
        return Task.CompletedTask;
    }

    public Task UpdateAsync(Consent consent, CancellationToken ct = default)
    {
        _store[consent.Id] = consent;
        return Task.CompletedTask;
    }

    public Task<bool> ExistsAsync(Guid id, CancellationToken ct = default) =>
        Task.FromResult(_store.ContainsKey(id));
}
