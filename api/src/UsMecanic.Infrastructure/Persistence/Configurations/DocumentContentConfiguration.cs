using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UsMecanic.Domain.Documents;
using UsMecanic.Infrastructure.Documents;

namespace UsMecanic.Infrastructure.Persistence.Configurations;

internal sealed class DocumentContentConfiguration : IEntityTypeConfiguration<DocumentContentRecord>
{
    public void Configure(EntityTypeBuilder<DocumentContentRecord> builder)
    {
        builder.ToTable("document_contents");
        builder.HasKey(c => c.DocumentId);
        builder.Property(c => c.DocumentId).ValueGeneratedNever();
        builder.Property(c => c.Content).HasColumnType("bytea").IsRequired();

        // Le contenu suit son document : supprimé avec lui.
        builder.HasOne<Document>()
            .WithOne()
            .HasForeignKey<DocumentContentRecord>(c => c.DocumentId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
