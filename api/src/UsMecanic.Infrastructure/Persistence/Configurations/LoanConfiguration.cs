using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UsMecanic.Domain.Costs;
using UsMecanic.Domain.Vehicles;

namespace UsMecanic.Infrastructure.Persistence.Configurations;

internal sealed class LoanConfiguration : IEntityTypeConfiguration<Loan>
{
    public void Configure(EntityTypeBuilder<Loan> builder)
    {
        builder.ToTable("loans");
        builder.HasKey(l => l.Id);
        builder.Property(l => l.Id).ValueGeneratedNever();

        builder.Property(l => l.Lender).HasMaxLength(Loan.MaxLenderLength);
        builder.Property(l => l.BorrowedAmount).HasPrecision(12, 2);
        builder.Property(l => l.MonthlyPayment).HasPrecision(12, 2);
        builder.Property(l => l.StartDate);
        builder.Property(l => l.DurationMonths);
        builder.Property(l => l.RecordedAt);

        builder.HasIndex(l => new { l.VehicleId, l.StartDate });

        // Les véhicules ne sont jamais supprimés (ils sortent du parc) : pas de suppression en cascade.
        builder.HasOne<Vehicle>()
            .WithMany()
            .HasForeignKey(l => l.VehicleId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
