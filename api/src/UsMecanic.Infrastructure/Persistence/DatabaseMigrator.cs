using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace UsMecanic.Infrastructure.Persistence;

public static class DatabaseMigrator
{
    /// <summary>Applique les migrations EF Core en attente (au démarrage de l'API, cf. Database:MigrateOnStartup).</summary>
    public static async Task MigrateDatabaseAsync(this IServiceProvider services, CancellationToken cancellationToken = default)
    {
        await using var scope = services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<UsMecanicDbContext>();
        await db.Database.MigrateAsync(cancellationToken);
    }
}
