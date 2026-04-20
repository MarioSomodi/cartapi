using Cart.Application.Abstractions.Auth;
using Cart.Application.Abstractions.Persistence;
using Cart.Application.Carts.Abstractions;
using Cart.Application.Carts.Shared;
using Cart.Application.Shared;
using MediatR;

namespace Cart.Application.Carts.GetCart;

public sealed record GetCartQuery : IRequest<Result<CartDto>>;

public sealed class GetCartQueryHandler(
    ICartRepository cartRepository,
    IRequestContext requestContext) : IRequestHandler<GetCartQuery, Result<CartDto>>
{
    public async Task<Result<CartDto>> Handle(GetCartQuery request, CancellationToken cancellationToken)
    {
        Result<RequestIdentity> identityResult = requestContext.GetRequiredIdentity();
        if (identityResult.IsFailure)
        {
            return Result<CartDto>.Failure(identityResult.Error);
        }

        RequestIdentity identity = identityResult.Value;

        DomainCart? cart = await cartRepository.GetActiveAsync(identity.TenantId, identity.SubjectId, cancellationToken);
        if (cart is null)
        {
            return Result<CartDto>.Failure(ApplicationErrors.Carts.CartNotFound);
        }

        return Result<CartDto>.Success(cart.ToDto());
    }
}
