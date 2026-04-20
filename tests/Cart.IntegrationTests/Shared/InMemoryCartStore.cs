using System.Collections.Concurrent;
using Cart.Application.Abstractions.Persistence;
using Cart.Application.Carts.Abstractions;
using DomainCart = Cart.Domain.Carts.Cart;

namespace Cart.IntegrationTests.Shared;

internal sealed class InMemoryCartStore : ICartRepository, IUnitOfWork
{
    private readonly ConcurrentDictionary<(string TenantId, string SubjectId), DomainCart> carts = new();

    public Task<DomainCart?> GetActiveAsync(string tenantId, string subjectId, CancellationToken cancellationToken = default)
    {
        carts.TryGetValue((tenantId, subjectId), out DomainCart? cart);
        return Task.FromResult(cart);
    }

    public Task AddAsync(DomainCart cart, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(cart);

        carts[(cart.TenantId, cart.SubjectId)] = cart;

        return Task.CompletedTask;
    }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult(1);
    }
}
