using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UsMecanic.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddDocumentCategories : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "document_categories",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_document_categories", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "ix_document_categories_name",
                table: "document_categories",
                column: "name",
                unique: true);

            // Valeurs initiales du référentiel, sans écraser l'existant.
            migrationBuilder.Sql("""
                INSERT INTO document_categories (id, name, is_active) VALUES ('019a2000-0000-7000-8000-000000000001', 'Carte grise', TRUE) ON CONFLICT DO NOTHING;
                INSERT INTO document_categories (id, name, is_active) VALUES ('019a2000-0000-7000-8000-000000000002', 'Attestation d''assurance', TRUE) ON CONFLICT DO NOTHING;
                INSERT INTO document_categories (id, name, is_active) VALUES ('019a2000-0000-7000-8000-000000000003', 'Procès-verbal de contrôle technique', TRUE) ON CONFLICT DO NOTHING;
                INSERT INTO document_categories (id, name, is_active) VALUES ('019a2000-0000-7000-8000-000000000004', 'Agrément ARS', TRUE) ON CONFLICT DO NOTHING;
                INSERT INTO document_categories (id, name, is_active) VALUES ('019a2000-0000-7000-8000-000000000005', 'Attestation de désinfection', TRUE) ON CONFLICT DO NOTHING;
                INSERT INTO document_categories (id, name, is_active) VALUES ('019a2000-0000-7000-8000-000000000006', 'Attestation de conformité funéraire', TRUE) ON CONFLICT DO NOTHING;
                INSERT INTO document_categories (id, name, is_active) VALUES ('019a2000-0000-7000-8000-000000000007', 'Qualification thermique (PSL)', TRUE) ON CONFLICT DO NOTHING;
                INSERT INTO document_categories (id, name, is_active) VALUES ('019a2000-0000-7000-8000-000000000008', 'Photo', TRUE) ON CONFLICT DO NOTHING;
                INSERT INTO document_categories (id, name, is_active) VALUES ('019a2000-0000-7000-8000-000000000009', 'Contrat de location ou de crédit', TRUE) ON CONFLICT DO NOTHING;
                INSERT INTO document_categories (id, name, is_active) VALUES ('019a2000-0000-7000-8000-000000000010', 'Autre', TRUE) ON CONFLICT DO NOTHING;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "document_categories");
        }
    }
}
