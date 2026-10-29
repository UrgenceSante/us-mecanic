using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UsMecanic.Domain.Problems;

namespace UsMecanic.Infrastructure.Persistence.Configurations;

/// <summary>Mapping commun des cinq référentiels des problèmes : une table par référentiel.</summary>
internal abstract class ProblemReferenceConfiguration<T>(string tableName) : IEntityTypeConfiguration<T>
    where T : ProblemReference
{
    public void Configure(EntityTypeBuilder<T> builder)
    {
        builder.ToTable(tableName);
        builder.HasKey(t => t.Id);
        builder.Property(t => t.Id).ValueGeneratedNever();

        builder.Property(t => t.Name).HasMaxLength(ProblemReference.MaxNameLength).IsRequired();
        builder.HasIndex(t => t.Name).IsUnique();

        builder.Property(t => t.IsActive);
        builder.Ignore(t => t.Kind);
    }
}

internal sealed class ProblemNatureConfiguration() : ProblemReferenceConfiguration<ProblemNature>("problem_natures");

internal sealed class ProblemElementConfiguration() : ProblemReferenceConfiguration<ProblemElement>("problem_elements");

internal sealed class ProblemActorConfiguration() : ProblemReferenceConfiguration<ProblemActor>("problem_actors");

internal sealed class ProblemActionTypeConfiguration() : ProblemReferenceConfiguration<ProblemActionType>("problem_action_types");

internal sealed class ProblemConstraintConfiguration() : ProblemReferenceConfiguration<ProblemConstraint>("problem_constraints");
