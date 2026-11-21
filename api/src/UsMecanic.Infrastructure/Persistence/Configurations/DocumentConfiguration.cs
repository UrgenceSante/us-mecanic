using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UsMecanic.Domain.Costs;
using UsMecanic.Domain.Documents;
using UsMecanic.Domain.Maintenance;
using UsMecanic.Domain.Vehicles;

namespace UsMecanic.Infrastructure.Persistence.Configurations;

internal sealed class DocumentConfiguration : IEntityTypeConfiguration<Document>
{
    public void Configure(EntityTypeBuilder<Document> builder)
    {
        builder.ToTable("documents", table => table.HasCheckConstraint(
            "ck_documents_attachment",
            "(category_id IS NOT NULL AND intervention_id IS NULL AND expense_id IS NULL) " +
            "OR (category_id IS NULL AND ((intervention_id IS NOT NULL) <> (expense_id IS NOT NULL)))"));
        builder.HasKey(d => d.Id);
        builder.Property(d => d.Id).ValueGeneratedNever();

        builder.Property(d => d.Label).HasMaxLength(Document.MaxLabelLength);
        builder.Property(d => d.FileName).HasMaxLength(Document.MaxFileNameLength).IsRequired();
        builder.Property(d => d.ContentType).HasConversion<string>().HasMaxLength(20);
        builder.Property(d => d.SizeBytes);
        builder.Property(d => d.AddedAt);

        builder.HasIndex(d => new { d.VehicleId, d.AddedAt });
        builder.HasIndex(d => d.InterventionId);
        builder.HasIndex(d => d.ExpenseId);
        builder.HasIndex(d => d.CategoryId);

        // Ni véhicule ni catégorie ne sont supprimés. Supprimer une intervention ou une dépense supprime ses
        // documents (et, par la table de contenu, leurs octets) dans la même transaction.
        builder.HasOne<Vehicle>()
            .WithMany()
            .HasForeignKey(d => d.VehicleId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<DocumentCategory>()
            .WithMany()
            .HasForeignKey(d => d.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<Intervention>()
            .WithMany()
            .HasForeignKey(d => d.InterventionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne<Expense>()
            .WithMany()
            .HasForeignKey(d => d.ExpenseId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
