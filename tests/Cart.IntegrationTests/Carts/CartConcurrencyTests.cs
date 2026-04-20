using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Cart.IntegrationTests.Shared;
using Microsoft.AspNetCore.Mvc.Testing;
using Shouldly;

namespace Cart.IntegrationTests.Carts;

public sealed class CartConcurrencyTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> factory;

    public CartConcurrencyTests(WebApplicationFactory<Program> factory)
    {
        this.factory = factory;
    }

    [Fact]
    public async Task AddItem_ShouldReturnConflictProblemDetails_WhenPersistenceDetectsConcurrentUpdate()
    {
        using WebApplicationFactory<Program> authenticatedFactory = factory.WithTestAuthenticationAndConcurrencyConflict();
        using HttpClient client = authenticatedFactory.CreateAuthenticatedClient(subjectId: "subject-1", tenantId: "tenant-1");

        HttpResponseMessage response = await client.PostAsJsonAsync(
            "/api/v1/cart/items",
            new
            {
                sku = "SKU-1",
                name = "Keyboard",
                quantity = 1,
                unitPrice = 10m,
                currency = "EUR"
            },
            TestContext.Current.CancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.Conflict);

        using JsonDocument problem = await response.ReadProblemAsync();
        problem.RootElement.GetProperty("code").GetString().ShouldBe("concurrency.conflict");
    }
}
