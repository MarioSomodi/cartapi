using Cart.Application.Abstractions.Auth;
using Cart.Application.Abstractions.Persistence;
using Cart.Application.Carts.Abstractions;
using Cart.Application.Carts.Shared;
using Cart.Application.Shared;
using MediatR;

namespace Cart.Application.Carts.ClearCart;

public sealed record ClearCartCommand : IRequest<Result<CartDto>>;

public sealed class ClearCartCommandHandler(
    ICartRepository cartRepository,
    IUnitOfWork unitOfWork,
    IRequestContext requestContext) : IRequestHandler<ClearCartCommand, Result<CartDto>>
{
    public async Task<Result<CartDto>> Handle(ClearCartCommand request, CancellationToken cancellationToken)
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

        cart.Clear();
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<CartDto>.Success(cart.ToDto());
    }
}
