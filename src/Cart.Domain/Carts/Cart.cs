namespace Cart.Domain.Carts;

public sealed class Cart
{
    private readonly List<CartItem> items = [];

    private Cart(string tenantId, string subjectId)
    {
        Id = Guid.NewGuid();
        TenantId = NormalizeRequired(tenantId, nameof(tenantId));
        SubjectId = NormalizeRequired(subjectId, nameof(subjectId));
        Status = CartStatus.Active;
        CreatedAtUtc = DateTime.UtcNow;
        UpdatedAtUtc = CreatedAtUtc;
        Version = 1;
    }

    public Guid Id { get; }

    public string TenantId { get; }

    public string SubjectId { get; }

    public CartStatus Status { get; }

    public DateTime CreatedAtUtc { get; }

    public DateTime UpdatedAtUtc { get; private set; }

    public int Version { get; private set; }

    public IReadOnlyCollection<CartItem> Items => items.AsReadOnly();

    public bool IsEmpty => items.Count == 0;

    public decimal TotalAmount => items.Sum(item => item.GetLineTotal());

    public static Cart Create(string tenantId, string subjectId)
    {
        return new Cart(tenantId, subjectId);
    }

    public CartItem AddItem(string sku, string name, int quantity, decimal unitPrice, string currency)
    {
        CartItem? existingItem = items.SingleOrDefault(item => string.Equals(item.Sku, sku?.Trim(), StringComparison.OrdinalIgnoreCase));

        if (existingItem is not null)
        {
            existingItem.IncreaseQuantity(quantity);
            existingItem.RefreshDisplayName(name);
            Touch();
            return existingItem;
        }

        CartItem cartItem = new(Id, sku, name, quantity, unitPrice, currency);
        items.Add(cartItem);
        Touch();

        return cartItem;
    }

    public void ChangeItemQuantity(Guid itemId, int quantity)
    {
        CartItem item = GetRequiredItem(itemId);
        item.ChangeQuantity(quantity);
        Touch();
    }

    public void RemoveItem(Guid itemId)
    {
        CartItem item = GetRequiredItem(itemId);
        items.Remove(item);
        Touch();
    }

    public void Clear()
    {
        if (items.Count == 0)
        {
            return;
        }

        items.Clear();
        Touch();
    }

    private CartItem GetRequiredItem(Guid itemId)
    {
        CartItem? item = items.SingleOrDefault(current => current.Id == itemId);

        if (item is null)
        {
            throw new InvalidOperationException($"Cart item '{itemId}' was not found.");
        }

        return item;
    }

    private void Touch()
    {
        UpdatedAtUtc = DateTime.UtcNow;
        Version++;
    }

    private static string NormalizeRequired(string value, string paramName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("Value is required.", paramName);
        }

        return value.Trim();
    }
}
