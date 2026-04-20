using Cart.Domain.Carts;
using Shouldly;
using DomainCart = Cart.Domain.Carts.Cart;

namespace Cart.Domain.Tests.Carts;

public sealed class CartTests
{
    [Fact]
    public void AddItem_ShouldMergeExistingSkuIntoSingleLine()
    {
        DomainCart cart = DomainCart.Create("tenant-1", "subject-1");

        cart.AddItem("SKU-1", "First name", 1, 10m, "eur");
        cart.AddItem(" sku-1 ", "Updated name", 2, 10m, "eur");

        cart.Items.Count.ShouldBe(1);

        CartItem item = cart.Items.Single();
        item.Quantity.ShouldBe(3);
        item.Name.ShouldBe("Updated name");
        item.Currency.ShouldBe("EUR");
        cart.TotalAmount.ShouldBe(30m);
    }

    [Fact]
    public void AddItem_ShouldRejectMergedSku_WhenUnitPriceChanges()
    {
        DomainCart cart = DomainCart.Create("tenant-1", "subject-1");
        cart.AddItem("SKU-1", "Keyboard", 1, 10m, "EUR");

        Should.Throw<CartItemSnapshotMismatchException>(() =>
            cart.AddItem("SKU-1", "Keyboard", 1, 12m, "EUR"));
    }

    [Fact]
    public void AddItem_ShouldRejectMergedSku_WhenCurrencyChanges()
    {
        DomainCart cart = DomainCart.Create("tenant-1", "subject-1");
        cart.AddItem("SKU-1", "Keyboard", 1, 10m, "EUR");

        Should.Throw<CartItemSnapshotMismatchException>(() =>
            cart.AddItem("SKU-1", "Keyboard", 1, 10m, "USD"));
    }
}
