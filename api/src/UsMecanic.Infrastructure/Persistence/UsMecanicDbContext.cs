using Microsoft.EntityFrameworkCore;
using UsMecanic.Domain.Deadlines;
using UsMecanic.Domain.Maintenance;
using UsMecanic.Domain.Mileage;
using UsMecanic.Domain.Vehicles;

namespace UsMecanic.Infrastructure.Persistence;

public sealed class UsMecanicDbContext(DbContextOptions<UsMecanicDbContext> options) : DbContext(options)
{
    public DbSet<Vehicle> Vehicles => Set<Vehicle>();

    public DbSet<VehicleType> VehicleTypes => Set<VehicleType>();

    public DbSet<Garage> Garages => Set<Garage>();

    public DbSet<Intervention> Interventions => Set<Intervention>();

    public DbSet<DeadlineType> DeadlineTypes => Set<DeadlineType>();

    public DbSet<DeadlineThresholds> GlobalDeadlineThresholds => Set<DeadlineThresholds>();

    public DbSet<VehicleDeadline> VehicleDeadlines => Set<VehicleDeadline>();

    public DbSet<MileageReading> MileageReadings => Set<MileageReading>();

    protected override void OnModelCreating(ModelBuilder modelBuilder) =>
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(UsMecanicDbContext).Assembly);
}
