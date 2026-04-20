using FluentValidation;

namespace Cart.Application.Carts.AddCartItem;

public sealed class AddCartItemCommandValidator : AbstractValidator<AddCartItemCommand>
{
    public AddCartItemCommandValidator()
    {
        RuleFor(command => command.Sku)
            .NotEmpty()
            .MaximumLength(128);

        RuleFor(command => command.Name)
            .NotEmpty()
            .MaximumLength(256);

        RuleFor(command => command.Quantity)
            .GreaterThan(0);

        RuleFor(command => command.UnitPrice)
            .GreaterThanOrEqualTo(0m);

        RuleFor(command => command.Currency)
            .NotEmpty()
            .Length(3);
    }
}
