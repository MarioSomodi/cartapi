namespace Cart.Application.Carts.Abstractions;

public interface ICartRepository
{
    Task<DomainCart?> GetActiveAsync(string tenantId, string subjectId, CancellationToken cancellationToken = default);

    Task AddAsync(DomainCart cart, CancellationToken cancellationToken = default);
}
