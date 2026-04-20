using FluentValidation;

namespace Cart.Application.Carts.UpdateCartItemQuantity;

public sealed class UpdateCartItemQuantityCommandValidator : AbstractValidator<UpdateCartItemQuantityCommand>
{
    public UpdateCartItemQuantityCommandValidator()
    {
        RuleFor(command => command.ItemId)
            .NotEmpty();

        RuleFor(command => command.Quantity)
            .GreaterThan(0);
    }
}
