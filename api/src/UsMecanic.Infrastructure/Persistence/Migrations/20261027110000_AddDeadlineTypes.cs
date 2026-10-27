using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UsMecanic.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddDeadlineTypes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "deadline_thresholds",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    due_soon_days = table.Column<int>(type: "integer", nullable: false),
                    due_soon_km = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_deadline_thresholds", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "deadline_types",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    default_interval_months = table.Column<int>(type: "integer", nullable: true),
                    default_interval_km = table.Column<int>(type: "integer", nullable: true),
                    due_soon_days = table.Column<int>(type: "integer", nullable: true),
                    due_soon_km = table.Column<int>(type: "integer", nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_deadline_types", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "ix_deadline_types_name",
                table: "deadline_types",
                column: "name",
                unique: true);

            // Données initiales : insérées ici (et non via HasData) pour ne pas dépendre du modèle EF.
            // Seuils globaux par défaut : 30 jours, 1 000 km.
            migrationBuilder.InsertData(
                table: "deadline_thresholds",
                columns: new[] { "id", "due_soon_days", "due_soon_km" },
                values: new object[] { new Guid("0d1a5e00-0000-4000-8000-000000000001"), 30, 1000 });

            // Types initiaux. Périodicité de 12 mois pour le contrôle technique, l'assurance et l'extincteur ;
            // les autres sont ponctuels (aucune périodicité par défaut).
            migrationBuilder.InsertData(
                table: "deadline_types",
                columns: new[] { "id", "name", "default_interval_months", "default_interval_km", "due_soon_days", "due_soon_km", "is_active" },
                values: new object[,]
                {
                    { new Guid("0d1a5e00-0000-4000-8000-000000000010"), "Contrôle technique", 12, null, null, null, true },
                    { new Guid("0d1a5e00-0000-4000-8000-000000000011"), "Assurance", 12, null, null, null, true },
                    { new Guid("0d1a5e00-0000-4000-8000-000000000012"), "Entretien constructeur", null, null, null, null, true },
                    { new Guid("0d1a5e00-0000-4000-8000-000000000013"), "Agrément ARS", null, null, null, null, true },
                    { new Guid("0d1a5e00-0000-4000-8000-000000000014"), "Matériel médical", null, null, null, null, true },
                    { new Guid("0d1a5e00-0000-4000-8000-000000000015"), "Oxygène", null, null, null, null, true },
                    { new Guid("0d1a5e00-0000-4000-8000-000000000016"), "Extincteur", 12, null, null, null, true }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "deadline_thresholds");

            migrationBuilder.DropTable(
                name: "deadline_types");
        }
    }
}
