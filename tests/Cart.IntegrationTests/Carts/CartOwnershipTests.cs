using System.Net;
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

        using JsonDocument problem = await response.ReadProblemAsync();
        problem.RootElement.GetProperty("code").GetString().ShouldBe("auth.missing_subject_id");
    }

    [Fact]
    public async Task GetCart_ShouldReturnUnauthorized_WhenAuthenticatedRequestIsMissingTenantId()
    {
        using WebApplicationFactory<Program> authenticatedFactory = factory.WithTestAuthenticationAndInMemoryCart();
        using HttpClient client = authenticatedFactory.CreateAuthenticatedClient(subjectId: "subject-1", tenantId: null);

        HttpResponseMessage response = await client.GetAsync("/api/v1/cart", TestContext.Current.CancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);

        using JsonDocument problem = await response.ReadProblemAsync();
        problem.RootElement.GetProperty("code").GetString().ShouldBe("auth.missing_tenant_id");
    }

    [Fact]
    public async Task GetCart_ShouldReturnNotFound_ForDifferentAuthenticatedSubject()
    {
        using WebApplicationFactory<Program> authenticatedFactory = factory.WithTestAuthenticationAndInMemoryCart();
        using HttpClient ownerClient = authenticatedFactory.CreateAuthenticatedClient(subjectId: "subject-1", tenantId: "tenant-1");
        using HttpClient otherClient = authenticatedFactory.CreateAuthenticatedClient(subjectId: "subject-2", tenantId: "tenant-1");

        await ownerClient.CreateCartAsync();

        HttpResponseMessage response = await otherClient.GetAsync("/api/v1/cart", TestContext.Current.CancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);

        using JsonDocument problem = await response.ReadProblemAsync();
        problem.RootElement.GetProperty("code").GetString().ShouldBe("carts.not_found");
    }

    [Fact]
    public async Task GetCart_ShouldReturnNotFound_ForDifferentAuthenticatedTenant()
    {
        using WebApplicationFactory<Program> authenticatedFactory = factory.WithTestAuthenticationAndInMemoryCart();
        using HttpClient ownerClient = authenticatedFactory.CreateAuthenticatedClient(subjectId: "subject-1", tenantId: "tenant-1");
        using HttpClient otherClient = authenticatedFactory.CreateAuthenticatedClient(subjectId: "subject-1", tenantId: "tenant-2");

        await ownerClient.CreateCartAsync();

        HttpResponseMessage response = await otherClient.GetAsync("/api/v1/cart", TestContext.Current.CancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);

        using JsonDocument problem = await response.ReadProblemAsync();
        problem.RootElement.GetProperty("code").GetString().ShouldBe("carts.not_found");
    }

    [Fact]
    public async Task GetCart_ShouldReturnCart_ForOwningIdentity()
    {
        using WebApplicationFactory<Program> authenticatedFactory = factory.WithTestAuthenticationAndInMemoryCart();
        using HttpClient client = authenticatedFactory.CreateAuthenticatedClient(subjectId: "subject-1", tenantId: "tenant-1");

        var addItemCart = await client.AddItemAsync("SKU-1", "Keyboard", 2, 50m, "EUR");

        HttpResponseMessage getCartResponse = await client.GetAsync("/api/v1/cart", TestContext.Current.CancellationToken);
        var cart = await getCartResponse.ReadCartAsync();

        cart.Id.ShouldBe(addItemCart.Id);
        cart.TenantId.ShouldBe("tenant-1");
        cart.SubjectId.ShouldBe("subject-1");
        cart.Items.Count.ShouldBe(1);
    }
}
