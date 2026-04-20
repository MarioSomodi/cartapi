using Cart.Application.Abstractions.Auth;
using Cart.Application.Abstractions.Persistence;
using Cart.Application.Carts.Abstractions;
using Cart.Application.Carts.Shared;
using Cart.Application.Shared;
using MediatR;

namespace Cart.Application.Carts.CreateCart;

public sealed record CreateCartCommand : IRequest<Result<CartDto>>;

public sealed class CreateCartCommandHandler(
    ICartRepository cartRepository,
    IUnitOfWork unitOfWork,
    IRequestContext requestContext) : IRequestHandler<CreateCartCommand, Result<CartDto>>
{
    public async Task<Result<CartDto>> Handle(CreateCartCommand request, CancellationToken cancellationToken)
    {
        Result<RequestIdentity> identityResult = requestContext.GetRequiredIdentity();
        if (identityResult.IsFailure)
        {
            return Result<CartDto>.Failure(identityResult.Error);
        }

        RequestIdentity identity = identityResult.Value;

        DomainCart? cart = await cartRepository.GetActiveAsync(identity.TenantId, identity.SubjectId, cancellationToken);
        if (cart is not null)
        {
            return Result<CartDto>.Success(cart.ToDto());
        }

        cart = DomainCart.Create(identity.TenantId, identity.SubjectId);
        await cartRepository.AddAsync(cart, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<CartDto>.Success(cart.ToDto());
    }
}
