using System.Diagnostics;
using Cart.Api.Middleware;
using Cart.Application.Shared;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;

namespace Cart.Api.Security;

public sealed class ProblemDetailsJwtBearerEvents(IProblemDetailsService problemDetailsService) : JwtBearerEvents
{
    public override async Task Challenge(JwtBearerChallengeContext context)
    {
        if (context.Response.HasStarted)
        {
            await base.Challenge(context);
            return;
        }

        context.HandleResponse();

        ProblemDetails problemDetails = new()
        {
            Status = StatusCodes.Status401Unauthorized,
            Title = "Unauthorized",
            Detail = ApplicationErrors.Auth.Unauthenticated.Message,
            Type = "https://httpstatuses.com/401",
            Instance = context.HttpContext.Request.Path
        };

        problemDetails.Extensions["code"] = ApplicationErrors.Auth.Unauthenticated.Code;
        problemDetails.Extensions["traceId"] = Activity.Current?.TraceId.ToString() ?? context.HttpContext.TraceIdentifier;

        string? correlationId = context.HttpContext.GetCorrelationId();
        if (!string.IsNullOrWhiteSpace(correlationId))
        {
            problemDetails.Extensions["correlationId"] = correlationId;
        }

        context.Response.StatusCode = StatusCodes.Status401Unauthorized;

        await problemDetailsService.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = context.HttpContext,
            ProblemDetails = problemDetails
        });
    }
}
