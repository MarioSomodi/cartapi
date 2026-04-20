using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Cart.Api.Controllers;

[ApiController]
[ApiVersionNeutral]
[AllowAnonymous]
[Route("health")]
public sealed class HealthController(HealthCheckService healthCheckService) : ControllerBase
{
    [HttpGet]
    public Task<IActionResult> GetHealthAsync(CancellationToken cancellationToken)
    {
        return GetHealthReportAsync(_ => true, cancellationToken);
    }

    [HttpGet("live")]
    public Task<IActionResult> GetLivenessAsync(CancellationToken cancellationToken)
    {
        return GetHealthReportAsync(registration => registration.Tags.Contains("live"), cancellationToken);
    }

    [HttpGet("ready")]
    public Task<IActionResult> GetReadinessAsync(CancellationToken cancellationToken)
    {
        return GetHealthReportAsync(registration => registration.Tags.Contains("ready"), cancellationToken);
    }

    private async Task<IActionResult> GetHealthReportAsync(
        Func<HealthCheckRegistration, bool> predicate,
        CancellationToken cancellationToken)
    {
        HealthReport report = await healthCheckService.CheckHealthAsync(predicate, cancellationToken);

        var response = new
        {
            status = report.Status.ToString(),
            totalDuration = report.TotalDuration,
            checks = report.Entries.Select(entry => new
            {
                name = entry.Key,
                status = entry.Value.Status.ToString(),
                duration = entry.Value.Duration,
                description = entry.Value.Description
            })
        };

        return report.Status switch
        {
            HealthStatus.Healthy => Ok(response),
            HealthStatus.Degraded => Ok(response),
            _ => StatusCode(StatusCodes.Status503ServiceUnavailable, response)
        };
    }
}
