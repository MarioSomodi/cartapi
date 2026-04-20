namespace Cart.Api.Contracts.Carts;

public sealed record CartItemResponse(
    Guid Id,
    Guid CartId,
    string Sku,
    string Name,
    int Quantity,
    decimal UnitPrice,
    string Currency);
