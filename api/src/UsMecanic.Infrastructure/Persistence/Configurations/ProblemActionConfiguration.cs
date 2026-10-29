using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UsMecanic.Domain.Maintenance;
using UsMecanic.Domain.Problems;

namespace UsMecanic.Infrastructure.Persistence.Configurations;

internal sealed class ProblemActionConfiguration : IEntityTypeConfiguration<ProblemAction>
{
    public void Configure(EntityTypeBuilder<ProblemAction> builder)
    {
        builder.ToTable("problem_actions");
        builder.HasKey(a => a.Id);
        builder.Property(a => a.Id).ValueGeneratedNever();

        builder.Property(a => a.DueDate);
        builder.Property(a => a.Comment).HasMaxLength(ProblemAction.MaxCommentLength);
        builder.Property(a => a.RecordedAt);
        builder.Property(a => a.ClosedAt);
        builder.Ignore(a => a.IsClosed);

        // Ni problème ni valeur de référentiel ne sont supprimés : pas de suppression en cascade.
        builder.HasOne<Problem>().WithMany(p => p.Actions).HasForeignKey(a => a.ProblemId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<ProblemActor>().WithMany().HasForeignKey(a => a.ActorId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<ProblemActionType>().WithMany().HasForeignKey(a => a.TypeId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<ProblemConstraint>().WithMany().HasForeignKey(a => a.ConstraintId).OnDelete(DeleteBehavior.Restrict);

        // Supprimer une intervention retire seulement le lien : l'action est conservée (suppression gérée par PostgreSQL).
        builder.HasOne<Intervention>().WithMany().HasForeignKey(a => a.InterventionId).OnDelete(DeleteBehavior.SetNull);
    }
}
