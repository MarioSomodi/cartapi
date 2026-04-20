using System.Security.Claims;
using Cart.Api.Configuration;
using Serilog;
using Serilog.Context;
using Serilog.Events;

namespace Cart.Api.Middleware;

public static class RequestContextLoggingMiddleware
{
    public static WebApplication UseRequestContextLogging(this WebApplication app)
    {
        app.Use(async (context, next) =>
        {
            string correlationId = context.GetCorrelationId() ?? "n/a";
            string? subjectId = context.User.FindFirstValue(ClaimTypes.NameIdentifier) ?? context.User.FindFirstValue("sub");
            string? tenantId = context.User.FindFirstValue("tenantId");

            using (LogContext.PushProperty(StructuredLogProperties.CorrelationId, correlationId))
            using (PushOptionalProperty(StructuredLogProperties.SubjectId, subjectId))
            using (PushOptionalProperty(StructuredLogProperties.TenantId, tenantId))
            {
                await next();
            }
        });

        app.UseSerilogRequestLogging(options =>
        {
            options.MessageTemplate = "HTTP {RequestMethod} {RequestPath} responded {StatusCode} in {Elapsed:0.0000} ms";
            options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
            {
                string correlationId = httpContext.GetCorrelationId() ?? "n/a";
                string? subjectId = httpContext.User.FindFirstValue(ClaimTypes.NameIdentifier) ?? httpContext.User.FindFirstValue("sub");
                string? tenantId = httpContext.User.FindFirstValue("tenantId");
                string? cartId = httpContext.GetCartId();

                diagnosticContext.Set(StructuredLogProperties.CorrelationId, correlationId);
                SetOptional(diagnosticContext, StructuredLogProperties.SubjectId, subjectId);
                SetOptional(diagnosticContext, StructuredLogProperties.TenantId, tenantId);
                SetOptional(diagnosticContext, StructuredLogProperties.CartId, cartId);
            };
            options.GetLevel = (httpContext, _, exception) =>
            {
                if (exception is not null || httpContext.Response.StatusCode >= 500)
                {
                    return LogEventLevel.Error;
                }

                return httpContext.Response.StatusCode >= 400
                    ? LogEventLevel.Warning
                    : LogEventLevel.Information;
            };
        });

        return app;
    }

    private static void SetOptional(IDiagnosticContext diagnosticContext, string propertyName, string? value)
    {
        if (!string.IsNullOrWhiteSpace(value))
        {
            diagnosticContext.Set(propertyName, value);
        }
    }

    private static IDisposable PushOptionalProperty(string propertyName, string? value) =>
        string.IsNullOrWhiteSpace(value)
            ? NullDisposable.Instance
            : LogContext.PushProperty(propertyName, value);

    private sealed class NullDisposable : IDisposable
    {
        public static readonly IDisposable Instance = new NullDisposable();

        public void Dispose()
        {
        }
    }
}
