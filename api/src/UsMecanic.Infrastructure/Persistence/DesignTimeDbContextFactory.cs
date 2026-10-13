using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace UsMecanic.Infrastructure.Persistence;

/// <summary>Utilisé uniquement par <c>dotnet ef</c> pour générer les migrations (aucune connexion ouverte).</summary>
internal sealed class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<UsMecanicDbContext>
{
    public UsMecanicDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<UsMecanicDbContext>();
        DependencyInjection.ConfigureDbContext(options, "Host=localhost;Database=us_mecanic");
        return new UsMecanicDbContext(options.Options);
    }
}
