using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UsMecanic.Domain.Deadlines;

namespace UsMecanic.Infrastructure.Persistence.Configurations;

internal sealed class DeadlineCompletionConfiguration : IEntityTypeConfiguration<DeadlineCompletion>
{
    public void Configure(EntityTypeBuilder<DeadlineCompletion> builder)
    {
        builder.ToTable("deadline_completions");
        builder.HasKey(c => c.Id);
        builder.Property(c => c.Id).ValueGeneratedNever();

        builder.Property(c => c.Date);
        builder.Property(c => c.Kilometers);
        builder.Property(c => c.Comment).HasMaxLength(DeadlineCompletion.MaxCommentLength);
        builder.Property(c => c.RecordedAt);

        builder.HasIndex(c => c.VehicleDeadlineId);

        // Les échéances ne sont jamais supprimées (elles sont closes) : pas de suppression en cascade.
        builder.HasOne<VehicleDeadline>()
            .WithMany()
            .HasForeignKey(c => c.VehicleDeadlineId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
