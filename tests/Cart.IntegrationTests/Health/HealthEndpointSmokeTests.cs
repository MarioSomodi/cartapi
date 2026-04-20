using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Shouldly;

namespace Cart.IntegrationTests.Health;

public sealed class HealthEndpointSmokeTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> factory;

    public HealthEndpointSmokeTests(WebApplicationFactory<Program> factory)
    {
        this.factory = factory.WithWebHostBuilder(builder => builder.UseEnvironment("Test"));
    }

    [Fact]
    public async Task LiveHealthChecks_ShouldReportHealthy()
    {
        using IServiceScope scope = factory.Services.CreateScope();
        HealthCheckService healthCheckService = scope.ServiceProvider.GetRequiredService<HealthCheckService>();

        HealthReport report = await healthCheckService.CheckHealthAsync(
            registration => registration.Tags.Contains("live"),
            TestContext.Current.CancellationToken);

        report.Status.ShouldBe(HealthStatus.Healthy);
    }
}
