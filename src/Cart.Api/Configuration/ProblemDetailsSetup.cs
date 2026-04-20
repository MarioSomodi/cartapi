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
                context.ProblemDetails.Extensions["traceId"] = context.HttpContext.TraceIdentifier;
            };
        });

        return services;
    }
}
