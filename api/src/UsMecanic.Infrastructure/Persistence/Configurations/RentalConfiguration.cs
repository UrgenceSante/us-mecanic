using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UsMecanic.Domain.Companies;
using UsMecanic.Domain.Rentals;
using UsMecanic.Domain.Vehicles;

namespace UsMecanic.Infrastructure.Persistence.Configurations;

internal sealed class RentalConfiguration : IEntityTypeConfiguration<Rental>
{
    public void Configure(EntityTypeBuilder<Rental> builder)
    {
        builder.ToTable("rentals");
        builder.HasKey(r => r.Id);
        builder.Property(r => r.Id).ValueGeneratedNever();

        // Jours entiers (type date), bornes incluses ; fin absente = location sans terme connu.
        builder.Property(r => r.StartDate);
        builder.Property(r => r.EndDate);

        builder.HasIndex(r => new { r.VehicleId, r.StartDate });

        // Ni véhicule ni société ne sont supprimés (sortie du parc, désactivation) : pas de suppression en cascade.
        builder.HasOne<Vehicle>()
            .WithMany()
            .HasForeignKey(r => r.VehicleId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<Company>()
            .WithMany()
            .HasForeignKey(r => r.TenantCompanyId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
