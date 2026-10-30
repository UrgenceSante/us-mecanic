using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UsMecanic.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddExpenseCategories : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "expense_categories",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_expense_categories", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "ix_expense_categories_name",
                table: "expense_categories",
                column: "name",
                unique: true);

            // Valeurs fictives de départ, à valider par le propriétaire. Sans écraser l'existant.
            migrationBuilder.Sql("""
                INSERT INTO expense_categories (id, name, is_active) VALUES ('019a1000-0000-7000-8000-000000000001', 'Nettoyage', TRUE) ON CONFLICT DO NOTHING;
                INSERT INTO expense_categories (id, name, is_active) VALUES ('019a1000-0000-7000-8000-000000000002', 'Contrôle technique', TRUE) ON CONFLICT DO NOTHING;
                INSERT INTO expense_categories (id, name, is_active) VALUES ('019a1000-0000-7000-8000-000000000003', 'Équipement', TRUE) ON CONFLICT DO NOTHING;
                INSERT INTO expense_categories (id, name, is_active) VALUES ('019a1000-0000-7000-8000-000000000004', 'Divers', TRUE) ON CONFLICT DO NOTHING;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "expense_categories");
        }
    }
}
