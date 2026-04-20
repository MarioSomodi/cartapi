using System.Diagnostics;
using Cart.Api.Middleware;
using Cart.Application.Shared;
using Microsoft.AspNetCore.Mvc;

namespace Cart.Api.Controllers;

internal static class ControllerBaseExtensions
{
    public static ObjectResult ToProblemDetails(this ControllerBase controller, Error error)
    {
        ArgumentNullException.ThrowIfNull(controller);
        ArgumentNullException.ThrowIfNull(error);

        int statusCode = GetStatusCode(error);
        ProblemDetails problemDetails = new()
        {
            Status = statusCode,
            Title = GetTitle(statusCode),
            Detail = error.Message,
            Type = $"https://httpstatuses.com/{statusCode}",
            Instance = controller.HttpContext.Request.Path
        };

        problemDetails.Extensions["code"] = error.Code;
        problemDetails.Extensions["traceId"] = Activity.Current?.TraceId.ToString() ?? controller.HttpContext.TraceIdentifier;

        string? correlationId = controller.HttpContext.GetCorrelationId();
        if (!string.IsNullOrWhiteSpace(correlationId))
        {
            problemDetails.Extensions["correlationId"] = correlationId;
        }

        return new ObjectResult(problemDetails)
        {
            StatusCode = statusCode
        };
    }

    private static int GetStatusCode(Error error) =>
        error.Code switch
        {
            "validation.failed" => StatusCodes.Status400BadRequest,
            "auth.unauthenticated" or "auth.missing_subject_id" or "auth.missing_tenant_id" => StatusCodes.Status401Unauthorized,
            "carts.not_found" or "carts.item_not_found" => StatusCodes.Status404NotFound,
            "concurrency.conflict" or "carts.item_snapshot_mismatch" => StatusCodes.Status409Conflict,
            _ => StatusCodes.Status400BadRequest
        };

    private static string GetTitle(int statusCode) =>
        statusCode switch
        {
            StatusCodes.Status400BadRequest => "Bad Request",
            StatusCodes.Status401Unauthorized => "Unauthorized",
            StatusCodes.Status404NotFound => "Not Found",
            StatusCodes.Status409Conflict => "Conflict",
            _ => "Request Failed"
        };
}
