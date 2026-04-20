using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Cart.IntegrationTests.Auth;

internal static class TestAuthenticationDefaults
{
    public const string Scheme = "Test";
    public const string EnabledHeaderName = "X-Test-Auth";
    public const string SubjectIdHeaderName = "X-Test-Sub";
    public const string TenantIdHeaderName = "X-Test-TenantId";
}

internal sealed class TestAuthenticationHandler(
    IOptionsMonitor<AuthenticationSchemeOptions> options,
    ILoggerFactory logger,
    UrlEncoder encoder) : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
{
    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (!Request.Headers.TryGetValue(TestAuthenticationDefaults.EnabledHeaderName, out var enabledHeader)
            || !string.Equals(enabledHeader.ToString(), "true", StringComparison.OrdinalIgnoreCase))
        {
            return Task.FromResult(AuthenticateResult.NoResult());
        }

        List<Claim> claims = [];

        if (Request.Headers.TryGetValue(TestAuthenticationDefaults.SubjectIdHeaderName, out var subjectIdHeader)
            && !string.IsNullOrWhiteSpace(subjectIdHeader.ToString()))
        {
            claims.Add(new Claim("sub", subjectIdHeader.ToString()));
        }

        if (Request.Headers.TryGetValue(TestAuthenticationDefaults.TenantIdHeaderName, out var tenantIdHeader)
            && !string.IsNullOrWhiteSpace(tenantIdHeader.ToString()))
        {
            claims.Add(new Claim("tenantId", tenantIdHeader.ToString()));
        }

        ClaimsIdentity identity = new(claims, TestAuthenticationDefaults.Scheme);
        ClaimsPrincipal principal = new(identity);
        AuthenticationTicket ticket = new(principal, TestAuthenticationDefaults.Scheme);

        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}
