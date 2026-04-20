using FluentValidation;

namespace Cart.Application.Carts.RemoveCartItem;

public sealed class RemoveCartItemCommandValidator : AbstractValidator<RemoveCartItemCommand>
{
    public RemoveCartItemCommandValidator()
    {
        RuleFor(command => command.ItemId)
            .NotEmpty();
    }
}
