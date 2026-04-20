using Cart.Domain.Carts;

namespace Cart.Application.Carts.Shared;

internal static class CartMappings
{
    public static CartDto ToDto(this DomainCart cart)
    {
        ArgumentNullException.ThrowIfNull(cart);

        return new CartDto(
            cart.Id,
            cart.TenantId,
            cart.SubjectId,
            cart.Status.ToString(),
            cart.CreatedAtUtc,
            cart.UpdatedAtUtc,
            cart.Version,
            cart.TotalAmount,
            cart.Items.Select(ToDto).ToArray());
    }

    private static CartItemDto ToDto(CartItem item)
    {
        return new CartItemDto(
            item.Id,
            item.CartId,
            item.Sku,
            item.Name,
            item.Quantity,
            item.UnitPrice,
            item.Currency);
    }
}
