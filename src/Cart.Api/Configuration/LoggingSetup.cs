using Elastic.CommonSchema;
using Elastic.CommonSchema.Serilog;
using Serilog;
using Serilog.Events;
using Serilog.Enrichers.Span;

namespace Cart.Api.Configuration;

public static class LoggingSetup
{
    public static WebApplicationBuilder AddSerilogLogging(this WebApplicationBuilder builder)
    {
        builder.Services.AddHttpContextAccessor();

        builder.Host.UseSerilog((context, services, loggerConfiguration) =>
        {
            ObservabilityOptions observability = ObservabilityOptions.Resolve(context.Configuration, context.HostingEnvironment);
            IHttpContextAccessor httpContextAccessor = services.GetRequiredService<IHttpContextAccessor>();

            loggerConfiguration
                .ReadFrom.Configuration(context.Configuration)
                .Enrich.FromLogContext()
                .Enrich.WithMachineName()
                .Enrich.WithProcessId()
                .Enrich.WithThreadId()
                .Enrich.WithSpan()
                .Enrich.WithEcsHttpContext(httpContextAccessor)
                .WriteTo.Console(CreateEcsTextFormatter(observability));
        });

        return builder;
    }

    private static EcsTextFormatter CreateEcsTextFormatter(ObservabilityOptions observability) =>
        new(new EcsTextFormatterConfiguration
        {
            LogEventPropertiesToFilter = new HashSet<string>(StringComparer.Ordinal)
            {
                StructuredLogProperties.CorrelationId,
                StructuredLogProperties.SubjectId,
                StructuredLogProperties.TenantId,
                StructuredLogProperties.CartId
            },
            MapCustom = (document, logEvent) =>
            {
                document.Service ??= new Service();
                document.Service.Name = observability.ServiceName;
                document.Service.Version = observability.ServiceVersion;
                document.Service.Environment = observability.DeploymentEnvironment;

                document.Event ??= new Event();
                document.Event.Dataset = observability.EventDataset;

                if (TryGetScalarString(logEvent, StructuredLogProperties.SubjectId, out string? subjectId))
                {
                    document.User ??= new User();
                    document.User.Id ??= subjectId;
                }

                if (TryGetScalarString(logEvent, StructuredLogProperties.CorrelationId, out string? correlationId))
                {
                    document.Labels ??= new Labels();
                    document.Labels["correlation_id"] = correlationId;
                }

                if (TryGetScalarString(logEvent, StructuredLogProperties.TenantId, out string? tenantId))
                {
                    document.Labels ??= new Labels();
                    document.Labels["tenant_id"] = tenantId;
                }

                if (TryGetScalarString(logEvent, StructuredLogProperties.CartId, out string? cartId))
                {
                    document.Labels ??= new Labels();
                    document.Labels["cart_id"] = cartId;
                }

                return document;
            }
        });

    private static bool TryGetScalarString(LogEvent logEvent, string propertyName, out string? value)
    {
        if (logEvent.Properties.TryGetValue(propertyName, out LogEventPropertyValue? propertyValue)
            && propertyValue is ScalarValue { Value: not null } scalarValue)
        {
            value = scalarValue.Value.ToString();
            return !string.IsNullOrWhiteSpace(value);
        }

        value = null;
        return false;
    }
}
