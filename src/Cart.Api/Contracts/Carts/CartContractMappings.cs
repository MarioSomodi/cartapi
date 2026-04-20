using Cart.Application.Carts.Shared;

namespace Cart.Api.Contracts.Carts;

internal static class CartContractMappings
{
    public static CartResponse ToResponse(this CartDto cart)
    {
        ArgumentNullException.ThrowIfNull(cart);

        return new CartResponse(
            cart.Id,
            cart.TenantId,
            cart.SubjectId,
            cart.Status,
            cart.CreatedAtUtc,
            cart.UpdatedAtUtc,
            cart.Version,
            cart.TotalAmount,
            cart.Items.Select(ToResponse).ToArray());
    }

    private static CartItemResponse ToResponse(CartItemDto item)
    {
        return new CartItemResponse(
            item.Id,
            item.CartId,
            item.Sku,
            item.Name,
            item.Quantity,
            item.UnitPrice,
            item.Currency);
    }
}
