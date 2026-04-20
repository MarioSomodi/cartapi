using Cart.Application.Abstractions.Persistence;
using Cart.Persistence.Context;

namespace Cart.Persistence;

public sealed class UnitOfWork(CartDbContext dbContext) : IUnitOfWork
{
    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return dbContext.SaveChangesAsync(cancellationToken);
    }
}
