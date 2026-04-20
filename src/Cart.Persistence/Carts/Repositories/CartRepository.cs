using Cart.Application.Carts.Abstractions;
using Cart.Domain.Carts;
using Cart.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace Cart.Persistence.Carts.Repositories;

public sealed class CartRepository(CartDbContext dbContext) : ICartRepository
{
    public Task<DomainCart?> GetActiveAsync(string tenantId, string subjectId, CancellationToken cancellationToken = default)
    {
        return dbContext.Carts
            .Include(cart => cart.Items)
            .SingleOrDefaultAsync(
                cart => cart.TenantId == tenantId
                    && cart.SubjectId == subjectId
                    && cart.Status == CartStatus.Active,
                cancellationToken);
    }

    public Task AddAsync(DomainCart cart, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(cart);

        return dbContext.Carts.AddAsync(cart, cancellationToken).AsTask();
    }
}
