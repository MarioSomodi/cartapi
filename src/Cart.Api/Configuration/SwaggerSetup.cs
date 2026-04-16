using Asp.Versioning.ApiExplorer;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Cart.Api.Configuration;

public static class SwaggerSetup
{
    public static IServiceCollection AddSwaggerDocumentation(this IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();
        services.AddTransient<IConfigureOptions<SwaggerGenOptions>, ConfigureSwaggerOptions>();
        services.AddSwaggerGen();

        return services;
    }

    public static WebApplication UseSwaggerDocumentation(this WebApplication app)
    {
        app.UseSwagger();
        app.UseSwaggerUI(options =>
        {
            IApiVersionDescriptionProvider provider = app.Services.GetRequiredService<IApiVersionDescriptionProvider>();

            if (provider.ApiVersionDescriptions.Count == 0)
            {
                options.SwaggerEndpoint("/swagger/v1/swagger.json", "V1");
                return;
            }

            foreach (ApiVersionDescription description in provider.ApiVersionDescriptions)
            {
                options.SwaggerEndpoint($"/swagger/{description.GroupName}/swagger.json", description.GroupName.ToUpperInvariant());
            }
        });

        return app;
    }
}

internal sealed class ConfigureSwaggerOptions(IApiVersionDescriptionProvider provider) : IConfigureOptions<SwaggerGenOptions>
{
    public void Configure(SwaggerGenOptions options)
    {
        if (provider.ApiVersionDescriptions.Count == 0)
        {
            options.SwaggerDoc("v1", CreateVersionInfo("v1", false));
            return;
        }

        foreach (ApiVersionDescription description in provider.ApiVersionDescriptions)
        {
            options.SwaggerDoc(description.GroupName, CreateVersionInfo(description.GroupName, description.IsDeprecated));
        }
    }

    private static OpenApiInfo CreateVersionInfo(string groupName, bool isDeprecated)
    {
        OpenApiInfo info = new()
        {
            Title = "Cart API",
            Version = groupName,
            Description = "Shopping cart API scaffolding."
        };

        if (isDeprecated)
        {
            info.Description = $"{info.Description} This API version has been deprecated.";
        }

        return info;
    }
}
