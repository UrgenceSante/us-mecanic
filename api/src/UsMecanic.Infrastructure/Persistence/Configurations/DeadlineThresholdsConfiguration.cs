using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UsMecanic.Domain.Deadlines;

namespace UsMecanic.Infrastructure.Persistence.Configurations;

internal sealed class DeadlineThresholdsConfiguration : IEntityTypeConfiguration<DeadlineThresholds>
{
    public void Configure(EntityTypeBuilder<DeadlineThresholds> builder)
    {
        builder.ToTable("deadline_thresholds");
        builder.HasKey(t => t.Id);
        builder.Property(t => t.Id).ValueGeneratedNever();

        builder.Property(t => t.DueSoonDays);
        builder.Property(t => t.DueSoonKm);
    }
}
