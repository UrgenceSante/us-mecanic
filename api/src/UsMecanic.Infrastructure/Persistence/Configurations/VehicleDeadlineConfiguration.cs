using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UsMecanic.Domain.Deadlines;
using UsMecanic.Domain.Vehicles;

namespace UsMecanic.Infrastructure.Persistence.Configurations;

internal sealed class VehicleDeadlineConfiguration : IEntityTypeConfiguration<VehicleDeadline>
{
    public void Configure(EntityTypeBuilder<VehicleDeadline> builder)
    {
        builder.ToTable("vehicle_deadlines");
        builder.HasKey(d => d.Id);
        builder.Property(d => d.Id).ValueGeneratedNever();

        builder.Property(d => d.IntervalMonths);
        builder.Property(d => d.IntervalKm);
        builder.Property(d => d.LastDoneDate);
        builder.Property(d => d.LastDoneKm);
        builder.Property(d => d.DueDate);
        builder.Property(d => d.DueKm);
        builder.Property(d => d.IsClosed);

        builder.Ignore(d => d.Schedule);

        // Un même type ne peut être actif (non clos) qu'une fois par véhicule ; sert aussi à lister par véhicule.
        builder.HasIndex(d => new { d.VehicleId, d.DeadlineTypeId })
            .IsUnique()
            .HasFilter("is_closed = false");

        // Ni les véhicules ni les types ne sont supprimés : pas de suppression en cascade.
        builder.HasOne<Vehicle>()
            .WithMany()
            .HasForeignKey(d => d.VehicleId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<DeadlineType>()
            .WithMany()
            .HasForeignKey(d => d.DeadlineTypeId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
