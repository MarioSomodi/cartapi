using Cart.Api.Security;
using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace Cart.Api.Configuration;

public static class AuthenticationSetup
{
    public static IServiceCollection AddJwtAuthentication(this IServiceCollection services)
    {
        services.AddScoped<ProblemDetailsJwtBearerEvents>();

        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        }).AddJwtBearer(options =>
        {
            options.MapInboundClaims = false;
            options.EventsType = typeof(ProblemDetailsJwtBearerEvents);
        });

        return services;
    }
}
