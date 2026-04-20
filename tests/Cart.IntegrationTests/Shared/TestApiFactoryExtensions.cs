using Cart.Application.Abstractions.Persistence;
using Cart.Application.Carts.Abstractions;
using Cart.IntegrationTests.Auth;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Cart.IntegrationTests.Shared;

internal static class TestApiFactoryExtensions
{
    public static WebApplicationFactory<Program> WithTestAuthenticationAndInMemoryCart(this WebApplicationFactory<Program> factory)
    {
        return factory.WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Test");
            builder.ConfigureTestServices(services =>
            {
                services.RemoveAll<ICartRepository>();
                services.RemoveAll<IUnitOfWork>();

                services.AddSingleton<InMemoryCartStore>();
                services.AddScoped<ICartRepository>(serviceProvider => serviceProvider.GetRequiredService<InMemoryCartStore>());
                services.AddScoped<IUnitOfWork>(serviceProvider => serviceProvider.GetRequiredService<InMemoryCartStore>());

                services.AddAuthentication(options =>
                {
                    options.DefaultAuthenticateScheme = TestAuthenticationDefaults.Scheme;
                    options.DefaultChallengeScheme = TestAuthenticationDefaults.Scheme;
                }).AddScheme<AuthenticationSchemeOptions, TestAuthenticationHandler>(TestAuthenticationDefaults.Scheme, _ => { });
            });
        });
    }

    public static HttpClient CreateAuthenticatedClient(
        this WebApplicationFactory<Program> factory,
        string? subjectId = "subject-1",
        string? tenantId = "tenant-1")
    {
        HttpClient client = factory.CreateClient();
        client.DefaultRequestHeaders.Add(TestAuthenticationDefaults.EnabledHeaderName, "true");

        if (!string.IsNullOrWhiteSpace(subjectId))
        {
            client.DefaultRequestHeaders.Add(TestAuthenticationDefaults.SubjectIdHeaderName, subjectId);
        }

        if (!string.IsNullOrWhiteSpace(tenantId))
        {
            client.DefaultRequestHeaders.Add(TestAuthenticationDefaults.TenantIdHeaderName, tenantId);
        }

        return client;
    }
}
