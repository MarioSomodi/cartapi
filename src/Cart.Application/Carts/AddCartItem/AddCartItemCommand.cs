using Cart.Application.Abstractions.Auth;
using Cart.Application.Abstractions.Persistence;
using Cart.Application.Carts.Abstractions;
using Cart.Application.Carts.Shared;
using Cart.Application.Shared;
using MediatR;

namespace Cart.Application.Carts.AddCartItem;

public sealed record AddCartItemCommand(
    string Sku,
    string Name,
    int Quantity,
    decimal UnitPrice,
    string Currency) : IRequest<Result<CartDto>>;

public sealed class AddCartItemCommandHandler(
    ICartRepository cartRepository,
    IUnitOfWork unitOfWork,
    IRequestContext requestContext) : IRequestHandler<AddCartItemCommand, Result<CartDto>>
{
    public async Task<Result<CartDto>> Handle(AddCartItemCommand request, CancellationToken cancellationToken)
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
            cart = DomainCart.Create(identity.TenantId, identity.SubjectId);
            await cartRepository.AddAsync(cart, cancellationToken);
        }

        cart.AddItem(request.Sku, request.Name, request.Quantity, request.UnitPrice, request.Currency);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<CartDto>.Success(cart.ToDto());
    }
}
