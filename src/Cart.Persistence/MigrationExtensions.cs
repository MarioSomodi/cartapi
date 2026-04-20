using Cart.Persistence.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Cart.Persistence;

public static class MigrationExtensions
{
    public static async Task ApplyDatabaseMigrationsAsync(this IServiceProvider services)
    {
        await using AsyncServiceScope scope = services.CreateAsyncScope();
        CartDbContext dbContext = scope.ServiceProvider.GetRequiredService<CartDbContext>();

        await dbContext.Database.MigrateAsync();
    }
}
