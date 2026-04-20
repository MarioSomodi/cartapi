using System.Security.Claims;
using Cart.Application.Abstractions.Auth;

namespace Cart.Api.Security;

public sealed class HttpRequestContext(IHttpContextAccessor httpContextAccessor) : IRequestContext
{
    public bool IsAuthenticated => HttpContext?.User?.Identity?.IsAuthenticated ?? false;

    public string? SubjectId => ResolveClaim(ClaimTypes.NameIdentifier) ?? ResolveClaim("sub");

    public string? TenantId => ResolveClaim("tenantId");

    private HttpContext? HttpContext => httpContextAccessor.HttpContext;

    private string? ResolveClaim(string claimType)
    {
        return HttpContext?.User?.FindFirstValue(claimType);
    }
}
