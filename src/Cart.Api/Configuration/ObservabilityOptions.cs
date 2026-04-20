using System.Reflection;

namespace Cart.Api.Configuration;

public sealed class ObservabilityOptions
{
    public const string SectionName = "Observability";

    public string ServiceName { get; init; } = "Cart.Api";
    public string ServiceNamespace { get; init; } = "abysalto";
    public string ServiceVersion { get; init; } = "1.0.0";
    public string DeploymentEnvironment { get; init; } = "Development";
    public string EventDataset { get; init; } = "abysalto.cart.api.log";
    public bool EnableConsoleTraceExporter { get; init; }

    public static ObservabilityOptions Resolve(IConfiguration configuration, IHostEnvironment environment)
    {
        ObservabilityOptions configured = configuration.GetSection(SectionName).Get<ObservabilityOptions>() ?? new();

        return new ObservabilityOptions
        {
            ServiceName = string.IsNullOrWhiteSpace(configured.ServiceName) ? "Cart.Api" : configured.ServiceName,
            ServiceNamespace = string.IsNullOrWhiteSpace(configured.ServiceNamespace) ? "abysalto" : configured.ServiceNamespace,
            ServiceVersion = string.IsNullOrWhiteSpace(configured.ServiceVersion) ? ResolveServiceVersion() : configured.ServiceVersion,
            DeploymentEnvironment = string.IsNullOrWhiteSpace(configured.DeploymentEnvironment) ? environment.EnvironmentName : configured.DeploymentEnvironment,
            EventDataset = string.IsNullOrWhiteSpace(configured.EventDataset) ? "abysalto.cart.api.log" : configured.EventDataset,
            EnableConsoleTraceExporter = configured.EnableConsoleTraceExporter
        };
    }

    private static string ResolveServiceVersion() =>
        Assembly.GetEntryAssembly()?.GetName().Version?.ToString(3)
        ?? Assembly.GetExecutingAssembly().GetName().Version?.ToString(3)
        ?? "1.0.0";
}
