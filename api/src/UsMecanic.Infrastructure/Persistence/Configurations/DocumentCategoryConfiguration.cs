using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UsMecanic.Domain.Documents;

namespace UsMecanic.Infrastructure.Persistence.Configurations;

internal sealed class DocumentCategoryConfiguration : IEntityTypeConfiguration<DocumentCategory>
{
    public void Configure(EntityTypeBuilder<DocumentCategory> builder)
    {
        builder.ToTable("document_categories");
        builder.HasKey(t => t.Id);
        builder.Property(t => t.Id).ValueGeneratedNever();

        builder.Property(t => t.Name).HasMaxLength(DocumentCategory.MaxNameLength).IsRequired();
        builder.HasIndex(t => t.Name).IsUnique();

        builder.Property(t => t.IsActive);
    }
}
