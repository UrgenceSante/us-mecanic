using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UsMecanic.Domain.Vehicles;

namespace UsMecanic.Infrastructure.Persistence.Configurations;

internal sealed class VehicleConfiguration : IEntityTypeConfiguration<Vehicle>
{
    public void Configure(EntityTypeBuilder<Vehicle> builder)
    {
        builder.ToTable("vehicles");
        builder.HasKey(v => v.Id);
        builder.Property(v => v.Id).ValueGeneratedNever();

        builder.Property(v => v.LicensePlate)
            .HasConversion(plate => plate.Value, value => LicensePlate.Create(value).Value)
            .HasMaxLength(9)
            .IsRequired();
        builder.HasIndex(v => v.LicensePlate).IsUnique();

        builder.Property(v => v.Vin)
            .HasConversion(vin => vin!.Value, value => Vin.Create(value).Value)
            .HasMaxLength(17);

        builder.Property(v => v.Brand).HasMaxLength(Vehicle.MaxTextLength);
        builder.Property(v => v.Model).HasMaxLength(Vehicle.MaxTextLength);
        builder.Property(v => v.InternalNumber).HasMaxLength(Vehicle.MaxTextLength);
        builder.Property(v => v.Status).HasConversion<string>().HasMaxLength(20);

        // Type facultatif ; un type référencé ne peut pas être supprimé (il est seulement désactivé).
        builder.HasOne<VehicleType>()
            .WithMany()
            .HasForeignKey(v => v.TypeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Property(v => v.CreatedAt);
        builder.Ignore(v => v.Details);
    }
}
