using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Cart.IntegrationTests.Shared;
using Microsoft.AspNetCore.Mvc.Testing;
using Shouldly;

namespace Cart.IntegrationTests.Carts;

public sealed class CartOwnershipTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> factory;

    public CartOwnershipTests(WebApplicationFactory<Program> factory)
    {
        this.factory = factory;
    }

    [Fact]
    public async Task GetCart_ShouldReturnUnauthorized_WhenAuthenticatedRequestIsMissingSubjectId()
    {
        using WebApplicationFactory<Program> authenticatedFactory = factory.WithTestAuthenticationAndInMemoryCart();
        using HttpClient client = authenticatedFactory.CreateAuthenticatedClient(subjectId: null, tenantId: "tenant-1");

        HttpResponseMessage response = await client.GetAsync("/api/v1/cart", TestContext.Current.CancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);

        JsonDocument problem = await ReadProblemAsync(response);
        problem.RootElement.GetProperty("code").GetString().ShouldBe("auth.missing_subject_id");
    }

    [Fact]
    public async Task GetCart_ShouldReturnUnauthorized_WhenAuthenticatedRequestIsMissingTenantId()
    {
        using WebApplicationFactory<Program> authenticatedFactory = factory.WithTestAuthenticationAndInMemoryCart();
        using HttpClient client = authenticatedFactory.CreateAuthenticatedClient(subjectId: "subject-1", tenantId: null);

        HttpResponseMessage response = await client.GetAsync("/api/v1/cart", TestContext.Current.CancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);

        JsonDocument problem = await ReadProblemAsync(response);
        problem.RootElement.GetProperty("code").GetString().ShouldBe("auth.missing_tenant_id");
    }

    [Fact]
    public async Task GetCart_ShouldReturnNotFound_ForDifferentAuthenticatedSubject()
    {
        using WebApplicationFactory<Program> authenticatedFactory = factory.WithTestAuthenticationAndInMemoryCart();
        using HttpClient ownerClient = authenticatedFactory.CreateAuthenticatedClient(subjectId: "subject-1", tenantId: "tenant-1");
        using HttpClient otherClient = authenticatedFactory.CreateAuthenticatedClient(subjectId: "subject-2", tenantId: "tenant-1");

        HttpResponseMessage createResponse = await ownerClient.PostAsync("/api/v1/cart", content: null, TestContext.Current.CancellationToken);
        createResponse.StatusCode.ShouldBe(HttpStatusCode.OK);

        HttpResponseMessage response = await otherClient.GetAsync("/api/v1/cart", TestContext.Current.CancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);

        JsonDocument problem = await ReadProblemAsync(response);
        problem.RootElement.GetProperty("code").GetString().ShouldBe("carts.not_found");
    }

    [Fact]
    public async Task GetCart_ShouldReturnNotFound_ForDifferentAuthenticatedTenant()
    {
        using WebApplicationFactory<Program> authenticatedFactory = factory.WithTestAuthenticationAndInMemoryCart();
        using HttpClient ownerClient = authenticatedFactory.CreateAuthenticatedClient(subjectId: "subject-1", tenantId: "tenant-1");
        using HttpClient otherClient = authenticatedFactory.CreateAuthenticatedClient(subjectId: "subject-1", tenantId: "tenant-2");

        HttpResponseMessage createResponse = await ownerClient.PostAsync("/api/v1/cart", content: null, TestContext.Current.CancellationToken);
        createResponse.StatusCode.ShouldBe(HttpStatusCode.OK);

        HttpResponseMessage response = await otherClient.GetAsync("/api/v1/cart", TestContext.Current.CancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);

        JsonDocument problem = await ReadProblemAsync(response);
        problem.RootElement.GetProperty("code").GetString().ShouldBe("carts.not_found");
    }

    [Fact]
    public async Task CartEndpoints_ShouldReturnOwnerCart_ForMatchingAuthenticatedIdentity()
    {
        using WebApplicationFactory<Program> authenticatedFactory = factory.WithTestAuthenticationAndInMemoryCart();
        using HttpClient client = authenticatedFactory.CreateAuthenticatedClient(subjectId: "subject-1", tenantId: "tenant-1");

        HttpResponseMessage addItemResponse = await client.PostAsJsonAsync(
            "/api/v1/cart/items",
            new
            {
                sku = "SKU-1",
                name = "Keyboard",
                quantity = 2,
                unitPrice = 50m,
                currency = "EUR"
            },
            TestContext.Current.CancellationToken);

        addItemResponse.StatusCode.ShouldBe(HttpStatusCode.OK);

        HttpResponseMessage getCartResponse = await client.GetAsync("/api/v1/cart", TestContext.Current.CancellationToken);
        getCartResponse.StatusCode.ShouldBe(HttpStatusCode.OK);

        JsonDocument payload = await ReadProblemAsync(getCartResponse);
        payload.RootElement.GetProperty("tenantId").GetString().ShouldBe("tenant-1");
        payload.RootElement.GetProperty("subjectId").GetString().ShouldBe("subject-1");
        payload.RootElement.GetProperty("items").GetArrayLength().ShouldBe(1);
    }

    private static async Task<JsonDocument> ReadProblemAsync(HttpResponseMessage response)
    {
        Stream content = await response.Content.ReadAsStreamAsync(TestContext.Current.CancellationToken);
        return await JsonDocument.ParseAsync(content, cancellationToken: TestContext.Current.CancellationToken);
    }
}
