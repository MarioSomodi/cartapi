using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace Cart.Api.Configuration;

public static class OpenTelemetrySetup
{
    public static IServiceCollection AddOpenTelemetrySupport(this IServiceCollection services, IConfiguration configuration, IHostEnvironment environment)
    {
        ObservabilityOptions observability = ObservabilityOptions.Resolve(configuration, environment);

        services.AddOpenTelemetry()
            .ConfigureResource(resource => resource
                .AddService(
                    serviceName: observability.ServiceName,
                    serviceNamespace: observability.ServiceNamespace,
                    serviceVersion: observability.ServiceVersion)
                .AddAttributes(
                [
                    new KeyValuePair<string, object>("deployment.environment", observability.DeploymentEnvironment)
                ]))
            .WithTracing(builder =>
            {
                builder.AddAspNetCoreInstrumentation(options => options.RecordException = true);
                builder.AddHttpClientInstrumentation();

                if (observability.EnableConsoleTraceExporter)
                {
                    builder.AddConsoleExporter();
                }
            });

        return services;
    }
}
