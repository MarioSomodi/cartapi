namespace Cart.Api.Contracts.Carts;

public sealed record AddCartItemRequest(
    string Sku,
    string Name,
    int Quantity,
    decimal UnitPrice,
    string Currency);
