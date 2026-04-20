using System.Net;
using System.Text.Json;
using Cart.Api.Middleware;
using Cart.IntegrationTests.Shared;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Shouldly;

namespace Cart.IntegrationTests.Observability;

public sealed class CorrelationIdTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> factory;

    public CorrelationIdTests(WebApplicationFactory<Program> factory)
    {
        this.factory = factory.WithWebHostBuilder(builder => builder.UseEnvironment("Test"));
    }

    [Fact]
    public async Task Health_ShouldGenerateCorrelationIdHeader_WhenRequestDoesNotProvideOne()
    {
        using HttpClient client = factory.CreateClient();

        HttpResponseMessage response = await client.GetAsync("/health/live", TestContext.Current.CancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        response.Headers.TryGetValues(CorrelationIdMiddleware.HeaderName, out IEnumerable<string>? values).ShouldBeTrue();
        values.ShouldNotBeNull();
        values.Single().ShouldNotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task ProblemDetails_ShouldEchoCorrelationId_WhenRequestProvidesOne()
    {
        using HttpClient client = factory.CreateClient();
        client.DefaultRequestHeaders.Add(CorrelationIdMiddleware.HeaderName, "corr-test-123");

        HttpResponseMessage response = await client.GetAsync("/api/v1/cart", TestContext.Current.CancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
        response.Headers.TryGetValues(CorrelationIdMiddleware.HeaderName, out IEnumerable<string>? values).ShouldBeTrue();
        values.ShouldNotBeNull();
        values.Single().ShouldBe("corr-test-123");

        using JsonDocument problem = await response.ReadProblemAsync();
        problem.RootElement.GetProperty("correlationId").GetString().ShouldBe("corr-test-123");
    }
}
