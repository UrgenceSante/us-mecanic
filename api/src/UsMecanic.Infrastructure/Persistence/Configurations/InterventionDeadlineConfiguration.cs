using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UsMecanic.Domain.Deadlines;
using UsMecanic.Domain.Maintenance;

namespace UsMecanic.Infrastructure.Persistence.Configurations;

internal sealed class InterventionDeadlineConfiguration : IEntityTypeConfiguration<InterventionDeadline>
{
    public void Configure(EntityTypeBuilder<InterventionDeadline> builder)
    {
        builder.ToTable("intervention_deadlines");
        builder.HasKey(d => new { d.InterventionId, d.VehicleDeadlineId });

        builder.HasOne<VehicleDeadline>()
            .WithMany()
            .HasForeignKey(d => d.VehicleDeadlineId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
