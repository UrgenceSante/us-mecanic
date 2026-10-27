using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UsMecanic.Domain.Mileage;
using UsMecanic.Domain.Vehicles;

namespace UsMecanic.Infrastructure.Persistence.Configurations;

internal sealed class MileageReadingConfiguration : IEntityTypeConfiguration<MileageReading>
{
    public void Configure(EntityTypeBuilder<MileageReading> builder)
    {
        builder.ToTable("mileage_readings");
        builder.HasKey(r => r.Id);
        builder.Property(r => r.Id).ValueGeneratedNever();

        builder.Property(r => r.Date);
        builder.Property(r => r.Kilometers);
        builder.Property(r => r.Reason).HasMaxLength(MileageReading.MaxReasonLength);
        builder.Property(r => r.RecordedAt);

        // L'historique est toujours lu par véhicule, trié par date.
        builder.HasIndex(r => new { r.VehicleId, r.Date });

        // Les véhicules ne sont jamais supprimés (ils sortent du parc) : pas de suppression en cascade.
        builder.HasOne<Vehicle>()
            .WithMany()
            .HasForeignKey(r => r.VehicleId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
