using Cart.Application.Abstractions.Persistence;
using Cart.Application.Carts.Abstractions;
using Cart.Persistence.Context;
using Cart.Persistence.Carts.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Cart.Persistence;

public static class DependencyInjection
{
    public static IServiceCollection AddPersistence(this IServiceCollection services, IConfiguration configuration)
    {
        string connectionString =
            configuration.GetConnectionString("CartDatabase")
            ?? throw new InvalidOperationException("Connection string 'CartDatabase' is missing.");

        services.AddDbContext<CartDbContext>(options =>
            options.UseNpgsql(connectionString, builder =>
                builder.MigrationsAssembly(typeof(CartDbContext).Assembly.FullName)));

        services.AddScoped<ICartRepository, CartRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        services.AddHealthChecks()
            .AddDbContextCheck<CartDbContext>("postgresql", tags: ["ready"]);

        return services;
    }
}
