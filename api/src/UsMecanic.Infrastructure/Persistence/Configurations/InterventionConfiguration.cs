using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UsMecanic.Domain.Maintenance;
using UsMecanic.Domain.Vehicles;

namespace UsMecanic.Infrastructure.Persistence.Configurations;

internal sealed class InterventionConfiguration : IEntityTypeConfiguration<Intervention>
{
    public void Configure(EntityTypeBuilder<Intervention> builder)
    {
        builder.ToTable("interventions");
        builder.HasKey(i => i.Id);
        builder.Property(i => i.Id).ValueGeneratedNever();

        builder.Property(i => i.Date);
        builder.Property(i => i.Kilometers);
        builder.Property(i => i.Work).HasMaxLength(Intervention.MaxWorkLength).IsRequired();
        builder.Property(i => i.RecordedAt);
        builder.Property(i => i.Amount).HasPrecision(12, 2);

        builder.HasIndex(i => new { i.VehicleId, i.Date });

        // Ni véhicule, ni garage, ni échéance ne sont supprimés : pas de suppression en cascade.
        builder.HasOne<Vehicle>()
            .WithMany()
            .HasForeignKey(i => i.VehicleId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<Garage>()
            .WithMany()
            .HasForeignKey(i => i.GarageId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(i => i.Deadlines)
            .WithOne()
            .HasForeignKey(d => d.InterventionId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
