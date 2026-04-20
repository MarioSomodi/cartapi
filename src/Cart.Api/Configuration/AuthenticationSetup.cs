using Cart.Api.Security;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

namespace Cart.Api.Configuration;

public static class AuthenticationSetup
{
    public static IServiceCollection AddJwtAuthentication(this IServiceCollection services, IConfiguration configuration)
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
            ConfigureTokenValidation(options, configuration);
        });

        return services;
    }

    private static void ConfigureTokenValidation(JwtBearerOptions options, IConfiguration configuration)
    {
        IConfigurationSection section = configuration.GetSection("Authentication:Schemes:Bearer");
        string? validIssuer = section["ValidIssuer"];
        string[] validAudiences = section.GetSection("ValidAudiences").Get<string[]>() ?? [];

        List<SecurityKey> signingKeys = [];

        foreach (IConfigurationSection signingKeySection in section.GetSection("SigningKeys").GetChildren())
        {
            string? encodedValue = signingKeySection["Value"];
            if (string.IsNullOrWhiteSpace(encodedValue))
            {
                continue;
            }

            byte[] keyBytes = Convert.FromBase64String(encodedValue);
            string? keyId = signingKeySection["Id"];

            SymmetricSecurityKey securityKey = new(keyBytes);
            if (!string.IsNullOrWhiteSpace(keyId))
            {
                securityKey.KeyId = keyId;
            }

            signingKeys.Add(securityKey);
        }

        if (signingKeys.Count == 0 && string.IsNullOrWhiteSpace(validIssuer) && validAudiences.Length == 0)
        {
            return;
        }

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = !string.IsNullOrWhiteSpace(validIssuer),
            ValidIssuer = validIssuer,
            ValidateAudience = validAudiences.Length > 0,
            ValidAudiences = validAudiences,
            ValidateIssuerSigningKey = signingKeys.Count > 0,
            IssuerSigningKeys = signingKeys,
            ValidateLifetime = true,
            NameClaimType = "sub",
            ClockSkew = TimeSpan.FromMinutes(1)
        };
    }
}
