using Microsoft.EntityFrameworkCore;
using UsMecanic.Domain.Vehicles;

namespace UsMecanic.Infrastructure.Persistence;

public sealed class UsMecanicDbContext(DbContextOptions<UsMecanicDbContext> options) : DbContext(options)
{
    public DbSet<Vehicle> Vehicles => Set<Vehicle>();

    public DbSet<VehicleType> VehicleTypes => Set<VehicleType>();

    protected override void OnModelCreating(ModelBuilder modelBuilder) =>
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(UsMecanicDbContext).Assembly);
}
