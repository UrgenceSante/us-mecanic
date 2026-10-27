using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UsMecanic.Domain.Deadlines;

namespace UsMecanic.Infrastructure.Persistence.Configurations;

internal sealed class DeadlineTypeConfiguration : IEntityTypeConfiguration<DeadlineType>
{
    public void Configure(EntityTypeBuilder<DeadlineType> builder)
    {
        builder.ToTable("deadline_types");
        builder.HasKey(t => t.Id);
        builder.Property(t => t.Id).ValueGeneratedNever();

        builder.Property(t => t.Name).HasMaxLength(DeadlineType.MaxNameLength).IsRequired();
        builder.HasIndex(t => t.Name).IsUnique();

        builder.Property(t => t.DefaultIntervalMonths);
        builder.Property(t => t.DefaultIntervalKm);
        builder.Property(t => t.DueSoonDays);
        builder.Property(t => t.DueSoonKm);
        builder.Property(t => t.IsActive);

        builder.Ignore(t => t.IsOneOff);
    }
}
