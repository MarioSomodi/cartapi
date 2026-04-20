namespace Cart.Application.Carts.Shared;

public sealed record CartDto(
    Guid Id,
    string TenantId,
    string SubjectId,
    string Status,
    DateTime CreatedAtUtc,
    DateTime UpdatedAtUtc,
    int Version,
    decimal TotalAmount,
    IReadOnlyCollection<CartItemDto> Items);

public sealed record CartItemDto(
    Guid Id,
    Guid CartId,
    string Sku,
    string Name,
    int Quantity,
    decimal UnitPrice,
    string Currency);
