using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Cart.Api.Configuration;

public static class HealthCheckSetup
{
    public static IServiceCollection AddCartHealthChecks(this IServiceCollection services)
    {
        services
            .AddHealthChecks()
            .AddCheck("self", () => HealthCheckResult.Healthy(), tags: ["live"]);

        return services;
    }
}
