using Cart.Application.Abstractions.Auth;
using Cart.Application.Abstractions.Persistence;
using Cart.Application.Carts.Abstractions;
using Cart.Application.Carts.Shared;
using Cart.Application.Shared;
using MediatR;

namespace Cart.Application.Carts.RemoveCartItem;

public sealed record RemoveCartItemCommand(Guid ItemId) : IRequest<Result<CartDto>>;

public sealed class RemoveCartItemCommandHandler(
    ICartRepository cartRepository,
    IUnitOfWork unitOfWork,
    IRequestContext requestContext) : IRequestHandler<RemoveCartItemCommand, Result<CartDto>>
{
    public async Task<Result<CartDto>> Handle(RemoveCartItemCommand request, CancellationToken cancellationToken)
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

        if (!cart.Items.Any(item => item.Id == request.ItemId))
        {
            return Result<CartDto>.Failure(ApplicationErrors.Carts.ItemNotFound);
        }

        cart.RemoveItem(request.ItemId);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<CartDto>.Success(cart.ToDto());
    }
}
