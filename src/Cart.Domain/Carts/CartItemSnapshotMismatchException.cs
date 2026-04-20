namespace Cart.Domain.Carts;

public sealed class CartItemSnapshotMismatchException(string sku)
    : InvalidOperationException($"The cart already contains SKU '{sku}' with a different unit price or currency.")
{
    public string Sku { get; } = sku;
}
