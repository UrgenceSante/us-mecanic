using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UsMecanic.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddProblemReferences : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "problem_action_types",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_problem_action_types", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "ix_problem_action_types_name",
                table: "problem_action_types",
                column: "name",
                unique: true);

            migrationBuilder.CreateTable(
                name: "problem_actors",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_problem_actors", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "ix_problem_actors_name",
                table: "problem_actors",
                column: "name",
                unique: true);

            migrationBuilder.CreateTable(
                name: "problem_constraints",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_problem_constraints", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "ix_problem_constraints_name",
                table: "problem_constraints",
                column: "name",
                unique: true);

            migrationBuilder.CreateTable(
                name: "problem_elements",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_problem_elements", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "ix_problem_elements_name",
                table: "problem_elements",
                column: "name",
                unique: true);

            migrationBuilder.CreateTable(
                name: "problem_natures",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_problem_natures", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "ix_problem_natures_name",
                table: "problem_natures",
                column: "name",
                unique: true);

            // Valeurs fictives de départ, à compléter par l'administrateur.
            migrationBuilder.Sql("""
                INSERT INTO problem_action_types (id, name, is_active) VALUES ('019a0000-0000-7000-8000-000000000001', 'Réparation', TRUE);
                INSERT INTO problem_action_types (id, name, is_active) VALUES ('019a0000-0000-7000-8000-000000000002', 'Remplacement', TRUE);
                INSERT INTO problem_action_types (id, name, is_active) VALUES ('019a0000-0000-7000-8000-000000000003', 'Contrôle', TRUE);
                INSERT INTO problem_actors (id, name, is_active) VALUES ('019a0000-0000-7000-8000-000000000004', 'Garage', TRUE);
                INSERT INTO problem_actors (id, name, is_active) VALUES ('019a0000-0000-7000-8000-000000000005', 'Atelier interne', TRUE);
                INSERT INTO problem_actors (id, name, is_active) VALUES ('019a0000-0000-7000-8000-000000000006', 'Constructeur', TRUE);
                INSERT INTO problem_constraints (id, name, is_active) VALUES ('019a0000-0000-7000-8000-000000000007', 'Immédiat', TRUE);
                INSERT INTO problem_constraints (id, name, is_active) VALUES ('019a0000-0000-7000-8000-000000000008', 'Sous 7 jours', TRUE);
                INSERT INTO problem_constraints (id, name, is_active) VALUES ('019a0000-0000-7000-8000-000000000009', 'Prochain entretien', TRUE);
                INSERT INTO problem_elements (id, name, is_active) VALUES ('019a0000-0000-7000-8000-000000000010', 'Moteur', TRUE);
                INSERT INTO problem_elements (id, name, is_active) VALUES ('019a0000-0000-7000-8000-000000000011', 'Freins', TRUE);
                INSERT INTO problem_elements (id, name, is_active) VALUES ('019a0000-0000-7000-8000-000000000012', 'Pneumatiques', TRUE);
                INSERT INTO problem_natures (id, name, is_active) VALUES ('019a0000-0000-7000-8000-000000000013', 'Mécanique', TRUE);
                INSERT INTO problem_natures (id, name, is_active) VALUES ('019a0000-0000-7000-8000-000000000014', 'Carrosserie', TRUE);
                INSERT INTO problem_natures (id, name, is_active) VALUES ('019a0000-0000-7000-8000-000000000015', 'Électrique', TRUE);
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "problem_action_types");

            migrationBuilder.DropTable(
                name: "problem_actors");

            migrationBuilder.DropTable(
                name: "problem_constraints");

            migrationBuilder.DropTable(
                name: "problem_elements");

            migrationBuilder.DropTable(
                name: "problem_natures");
        }
    }
}
