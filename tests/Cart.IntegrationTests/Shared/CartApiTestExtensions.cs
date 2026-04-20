using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Cart.Api.Contracts.Carts;
using Shouldly;

namespace Cart.IntegrationTests.Shared;

internal static class CartApiTestExtensions
{
    public static async Task<CartResponse> CreateCartAsync(this HttpClient client)
    {
        HttpResponseMessage response = await client.PostAsync("/api/v1/cart", content: null, TestContext.Current.CancellationToken);
        return await response.ReadCartAsync();
    }

    public static async Task<CartResponse> AddItemAsync(
        this HttpClient client,
        string sku,
        string name,
        int quantity,
        decimal unitPrice,
        string currency)
    {
        HttpResponseMessage response = await client.PostAsJsonAsync(
            "/api/v1/cart/items",
            new
            {
                sku,
                name,
                quantity,
                unitPrice,
                currency
            },
            TestContext.Current.CancellationToken);

        return await response.ReadCartAsync();
    }

    public static async Task<CartResponse> ReadCartAsync(this HttpResponseMessage response)
    {
        response.StatusCode.ShouldBe(HttpStatusCode.OK);

        CartResponse? cart = await response.Content.ReadFromJsonAsync<CartResponse>(TestContext.Current.CancellationToken);
        cart.ShouldNotBeNull();

        return cart;
    }

    public static async Task<JsonDocument> ReadProblemAsync(this HttpResponseMessage response)
    {
        Stream content = await response.Content.ReadAsStreamAsync(TestContext.Current.CancellationToken);
        return await JsonDocument.ParseAsync(content, cancellationToken: TestContext.Current.CancellationToken);
    }
}
