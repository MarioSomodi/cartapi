using Cart.Application.Abstractions.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Cart.IntegrationTests.Shared;

internal sealed class ConflictOnSaveUnitOfWork : IUnitOfWork
{
    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        throw new DbUpdateConcurrencyException("Simulated concurrency conflict for integration testing.");
    }
}
