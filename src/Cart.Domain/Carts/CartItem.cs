namespace Cart.Domain.Carts;

public sealed class CartItem
{
    private CartItem()
    {
    }

    internal CartItem(Guid cartId, string sku, string name, int quantity, decimal unitPrice, string currency)
    {
        Id = Guid.NewGuid();
        CartId = cartId;
        Sku = NormalizeRequired(sku, nameof(sku));
        Name = NormalizeRequired(name, nameof(name));
        Quantity = ValidateQuantity(quantity);
        UnitPrice = ValidateUnitPrice(unitPrice);
        Currency = NormalizeCurrency(currency);
    }

    public Guid Id { get; private set; }

    public Guid CartId { get; private set; }

    public string Sku { get; private set; } = string.Empty;

    public string Name { get; private set; } = string.Empty;

    public int Quantity { get; private set; }

    public decimal UnitPrice { get; private set; }

    public string Currency { get; private set; } = string.Empty;

    public void IncreaseQuantity(int quantity)
    {
        Quantity += ValidateQuantity(quantity);
    }

    public void ChangeQuantity(int quantity)
    {
        Quantity = ValidateQuantity(quantity);
    }

    public decimal GetLineTotal() => Quantity * UnitPrice;

    internal void RefreshDisplayName(string name)
    {
        Name = NormalizeRequired(name, nameof(name));
    }

    private static string NormalizeRequired(string value, string paramName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("Value is required.", paramName);
        }

        return value.Trim();
    }

    private static int ValidateQuantity(int quantity)
    {
        if (quantity <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(quantity), quantity, "Quantity must be positive.");
        }

        return quantity;
    }

    private static decimal ValidateUnitPrice(decimal unitPrice)
    {
        if (unitPrice < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(unitPrice), unitPrice, "Unit price must be zero or greater.");
        }

        return unitPrice;
    }

    private static string NormalizeCurrency(string currency)
    {
        string normalized = NormalizeRequired(currency, nameof(currency)).ToUpperInvariant();

        if (normalized.Length != 3)
        {
            throw new ArgumentException("Currency must be a three-letter ISO code.", nameof(currency));
        }

        return normalized;
    }
}
