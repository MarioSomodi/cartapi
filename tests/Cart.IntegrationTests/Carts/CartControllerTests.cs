using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Cart.IntegrationTests.Shared;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Shouldly;

namespace Cart.IntegrationTests.Carts;

public sealed class CartControllerTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> factory;
    private readonly HttpClient client;

    public CartControllerTests(WebApplicationFactory<Program> factory)
    {
        this.factory = factory;
        client = factory
            .WithWebHostBuilder(builder => builder.UseEnvironment("Test"))
            .CreateClient();
    }

    [Fact]
    public async Task GetCart_ShouldReturnUnauthorizedProblemDetails_WhenRequestIsUnauthenticated()
    {
        HttpResponseMessage response = await client.GetAsync("/api/v1/cart", TestContext.Current.CancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);

        JsonDocument problem = await ReadProblemAsync(response);
        problem.RootElement.GetProperty("code").GetString().ShouldBe("auth.unauthenticated");
    }

    [Fact]
    public async Task AddItem_ShouldReturnBadRequest_WhenPayloadIsInvalid()
    {
        using WebApplicationFactory<Program> authenticatedFactory = factory.WithTestAuthenticationAndInMemoryCart();
        using HttpClient authenticatedClient = authenticatedFactory.CreateAuthenticatedClient();

        HttpResponseMessage response = await authenticatedClient.PostAsJsonAsync(
                "/api/v1/cart/items",
                new
                {
                    sku = string.Empty,
                    name = string.Empty,
                    quantity = 0,
                    unitPrice = -1m,
                    currency = "EU"
                },
                TestContext.Current.CancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);

        JsonDocument problem = await ReadProblemAsync(response);
        problem.RootElement.GetProperty("code").GetString().ShouldBe("validation.failed");
    }

    [Fact]
    public async Task RemoveItem_ShouldReturnBadRequest_WhenItemIdIsEmpty()
    {
        using WebApplicationFactory<Program> authenticatedFactory = factory.WithTestAuthenticationAndInMemoryCart();
        using HttpClient authenticatedClient = authenticatedFactory.CreateAuthenticatedClient();

        HttpResponseMessage response = await authenticatedClient.DeleteAsync(
                $"/api/v1/cart/items/{Guid.Empty}",
                TestContext.Current.CancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);

        JsonDocument problem = await ReadProblemAsync(response);
        problem.RootElement.GetProperty("code").GetString().ShouldBe("validation.failed");
    }

    private static async Task<JsonDocument> ReadProblemAsync(HttpResponseMessage response)
    {
        Stream content = await response.Content.ReadAsStreamAsync(TestContext.Current.CancellationToken);
        return await JsonDocument.ParseAsync(content, cancellationToken: TestContext.Current.CancellationToken);
    }
}
