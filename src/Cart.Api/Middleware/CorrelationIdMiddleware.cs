using Microsoft.Extensions.Primitives;

namespace Cart.Api.Middleware;

public static class CorrelationIdMiddleware
{
    public const string HeaderName = "X-Correlation-ID";

    public static WebApplication UseCorrelationId(this WebApplication app)
    {
        app.Use(async (context, next) =>
        {
            if (!context.Request.Headers.TryGetValue(HeaderName, out StringValues correlationId)
                || string.IsNullOrWhiteSpace(correlationId))
            {
                correlationId = Guid.NewGuid().ToString("N");
            }

            string resolvedCorrelationId = correlationId.ToString();

            context.Items[RequestContextItems.CorrelationId] = resolvedCorrelationId;
            context.Response.Headers[HeaderName] = resolvedCorrelationId;

            await next();
        });

        return app;
    }
}
