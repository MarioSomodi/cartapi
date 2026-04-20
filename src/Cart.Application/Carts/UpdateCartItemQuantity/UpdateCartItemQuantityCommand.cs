using Cart.Application.Abstractions.Auth;
using Cart.Application.Abstractions.Persistence;
using Cart.Application.Carts.Abstractions;
using Cart.Application.Carts.Shared;
using Cart.Application.Shared;
using MediatR;

namespace Cart.Application.Carts.UpdateCartItemQuantity;

public sealed record UpdateCartItemQuantityCommand(Guid ItemId, int Quantity) : IRequest<Result<CartDto>>;

public sealed class UpdateCartItemQuantityCommandHandler(
    ICartRepository cartRepository,
    IUnitOfWork unitOfWork,
    IRequestContext requestContext) : IRequestHandler<UpdateCartItemQuantityCommand, Result<CartDto>>
{
    public async Task<Result<CartDto>> Handle(UpdateCartItemQuantityCommand request, CancellationToken cancellationToken)
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

        cart.ChangeItemQuantity(request.ItemId, request.Quantity);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<CartDto>.Success(cart.ToDto());
    }
}
