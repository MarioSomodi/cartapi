using System.Diagnostics;
using Cart.Api.Middleware;
using Microsoft.AspNetCore.Mvc;

namespace Cart.Api.Configuration;

public static class ProblemDetailsSetup
{
    public static IServiceCollection AddProblemDetailsSupport(this IServiceCollection services)
    {
        services.AddProblemDetails(options =>
        {
            options.CustomizeProblemDetails = context =>
            {
                context.ProblemDetails.Instance ??= context.HttpContext.Request.Path;
                context.ProblemDetails.Extensions["traceId"] = Activity.Current?.TraceId.ToString() ?? context.HttpContext.TraceIdentifier;

                string? correlationId = context.HttpContext.GetCorrelationId();
                if (!string.IsNullOrWhiteSpace(correlationId))
                {
                    context.ProblemDetails.Extensions["correlationId"] = correlationId;
                }
            };
        });

        return services;
    }
}
