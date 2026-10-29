using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UsMecanic.Domain.Problems;
using UsMecanic.Domain.Vehicles;

namespace UsMecanic.Infrastructure.Persistence.Configurations;

internal sealed class ProblemConfiguration : IEntityTypeConfiguration<Problem>
{
    public void Configure(EntityTypeBuilder<Problem> builder)
    {
        builder.ToTable("problems");
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Id).ValueGeneratedNever();

        builder.Property(p => p.Date);
        builder.Property(p => p.Description).HasMaxLength(Problem.MaxDescriptionLength).IsRequired();
        builder.Property(p => p.Author).HasMaxLength(Problem.MaxAuthorLength).IsRequired();
        builder.Property(p => p.Status).HasConversion<string>().HasMaxLength(20);
        builder.Property(p => p.RecordedAt);

        // Analyse (1-1) : colonnes nullables, renseignées au passage à « Analysé ».
        builder.Property(p => p.AnalysisText).HasMaxLength(Problem.MaxAnalysisLength);
        builder.Property(p => p.Immobilized);
        builder.Property(p => p.AnalyzedAt);
        builder.Ignore(p => p.Analysis);

        // Les valeurs de référentiel ne sont jamais supprimées (seulement désactivées).
        builder.HasOne<ProblemNature>().WithMany().HasForeignKey(p => p.NatureId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<ProblemElement>().WithMany().HasForeignKey(p => p.ElementId).OnDelete(DeleteBehavior.Restrict);

        // Les problèmes sont toujours lus par véhicule, triés par date.
        builder.HasIndex(p => new { p.VehicleId, p.Date });

        // Les véhicules ne sont jamais supprimés (ils sortent du parc) : pas de suppression en cascade.
        builder.HasOne<Vehicle>()
            .WithMany()
            .HasForeignKey(p => p.VehicleId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
