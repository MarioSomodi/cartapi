using MediatR;

namespace Cart.Api.Configuration;

public static class ApplicationSetup
{
    public static IServiceCollection AddApplicationLayer(this IServiceCollection services)
    {
        services.AddMediatR(configuration =>
            configuration.RegisterServicesFromAssembly(typeof(Cart.Application.ApplicationMarker).Assembly));

        return services;
    }
}
