namespace Cart.Api.Contracts.Carts;

public sealed record CartResponse(
    Guid Id,
    string TenantId,
    string SubjectId,
    string Status,
    DateTime CreatedAtUtc,
    DateTime UpdatedAtUtc,
    int Version,
    decimal TotalAmount,
    IReadOnlyCollection<CartItemResponse> Items);
