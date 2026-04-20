using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Cart.Api.Contracts.Carts;
using Cart.IntegrationTests.Shared;
using Microsoft.AspNetCore.Mvc.Testing;
using Shouldly;

namespace Cart.IntegrationTests.Carts;

public sealed class CartFlowTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> factory;

    public CartFlowTests(WebApplicationFactory<Program> factory)
    {
        this.factory = factory;
    }

    [Fact]
    public async Task CreateCart_ShouldReturnExistingCart_WhenCartAlreadyExists()
    {
        using WebApplicationFactory<Program> authenticatedFactory = factory.WithTestAuthenticationAndInMemoryCart();
        using HttpClient client = authenticatedFactory.CreateAuthenticatedClient(subjectId: "subject-1", tenantId: "tenant-1");

        CartResponse createdCart = await client.CreateCartAsync();
        CartResponse updatedCart = await client.AddItemAsync("SKU-1", "Keyboard", 2, 50m, "EUR");

        HttpResponseMessage createAgainResponse = await client.PostAsync("/api/v1/cart", content: null, TestContext.Current.CancellationToken);
        CartResponse existingCart = await createAgainResponse.ReadCartAsync();

        existingCart.Id.ShouldBe(createdCart.Id);
        existingCart.Id.ShouldBe(updatedCart.Id);
        existingCart.Items.Count.ShouldBe(1);
        existingCart.TotalAmount.ShouldBe(100m);
    }

    [Fact]
    public async Task UpdateItemQuantity_ShouldUpdateExistingItem()
    {
        using WebApplicationFactory<Program> authenticatedFactory = factory.WithTestAuthenticationAndInMemoryCart();
        using HttpClient client = authenticatedFactory.CreateAuthenticatedClient(subjectId: "subject-1", tenantId: "tenant-1");

        CartResponse addedCart = await client.AddItemAsync("SKU-1", "Keyboard", 2, 50m, "EUR");
        Guid itemId = addedCart.Items.Single().Id;

        HttpResponseMessage response = await client.PutAsJsonAsync(
            $"/api/v1/cart/items/{itemId}",
            new { quantity = 5 },
            TestContext.Current.CancellationToken);

        CartResponse updatedCart = await response.ReadCartAsync();
        CartItemResponse item = updatedCart.Items.Single();

        item.Id.ShouldBe(itemId);
        item.Quantity.ShouldBe(5);
        updatedCart.TotalAmount.ShouldBe(250m);
    }

    [Fact]
    public async Task UpdateItemQuantity_ShouldReturnNotFound_WhenItemDoesNotExist()
    {
        using WebApplicationFactory<Program> authenticatedFactory = factory.WithTestAuthenticationAndInMemoryCart();
        using HttpClient client = authenticatedFactory.CreateAuthenticatedClient(subjectId: "subject-1", tenantId: "tenant-1");

        await client.CreateCartAsync();

        HttpResponseMessage response = await client.PutAsJsonAsync(
            $"/api/v1/cart/items/{Guid.NewGuid()}",
            new { quantity = 2 },
            TestContext.Current.CancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);

        using JsonDocument problem = await response.ReadProblemAsync();
        problem.RootElement.GetProperty("code").GetString().ShouldBe("carts.item_not_found");
    }

    [Fact]
    public async Task RemoveItem_ShouldRemoveExistingItem()
    {
        using WebApplicationFactory<Program> authenticatedFactory = factory.WithTestAuthenticationAndInMemoryCart();
        using HttpClient client = authenticatedFactory.CreateAuthenticatedClient(subjectId: "subject-1", tenantId: "tenant-1");

        CartResponse addedCart = await client.AddItemAsync("SKU-1", "Keyboard", 2, 50m, "EUR");
        Guid itemId = addedCart.Items.Single().Id;

        HttpResponseMessage response = await client.DeleteAsync(
            $"/api/v1/cart/items/{itemId}",
            TestContext.Current.CancellationToken);

        CartResponse cart = await response.ReadCartAsync();

        cart.Items.ShouldBeEmpty();
        cart.TotalAmount.ShouldBe(0m);
    }

    [Fact]
    public async Task ClearCart_ShouldRemoveAllItems()
    {
        using WebApplicationFactory<Program> authenticatedFactory = factory.WithTestAuthenticationAndInMemoryCart();
        using HttpClient client = authenticatedFactory.CreateAuthenticatedClient(subjectId: "subject-1", tenantId: "tenant-1");

        await client.AddItemAsync("SKU-1", "Keyboard", 2, 50m, "EUR");
        await client.AddItemAsync("SKU-2", "Mouse", 1, 25m, "EUR");

        HttpResponseMessage response = await client.DeleteAsync("/api/v1/cart", TestContext.Current.CancellationToken);

        CartResponse cart = await response.ReadCartAsync();

        cart.Items.ShouldBeEmpty();
        cart.TotalAmount.ShouldBe(0m);
    }
}
