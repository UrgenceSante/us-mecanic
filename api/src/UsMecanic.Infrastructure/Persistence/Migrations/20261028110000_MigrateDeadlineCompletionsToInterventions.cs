using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UsMecanic.Infrastructure.Persistence.Migrations
{
    /// <summary>
    /// Reprise des réalisations d'échéance existantes dans le carnet d'entretien, puis suppression de la table
    /// <c>deadline_completions</c> (« Marquer réalisée » crée désormais une intervention).
    /// Up : chaque réalisation devient une intervention de même identifiant (ce qui rend la reprise idempotente),
    /// même date, même km, travaux = commentaire sinon nom du type d'échéance, date de saisie conservée, véhicule de
    /// l'échéance, sans garage, liée à son échéance. Down : recrée la table et y remet les interventions liées à exactement
    /// une échéance (le commentaire d'origine n'est restitué que s'il diffère du nom du type) ; les interventions
    /// ne sont pas supprimées, et celles créées depuis la migration y sont aussi reprises.
    /// </summary>
    public partial class MigrateDeadlineCompletionsToInterventions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                INSERT INTO interventions (id, vehicle_id, date, kilometers, garage_id, work, recorded_at)
                SELECT c.id,
                       d.vehicle_id,
                       c.date,
                       c.kilometers,
                       NULL,
                       LEFT(COALESCE(NULLIF(BTRIM(c.comment), ''), t.name), 2000),
                       c.recorded_at
                FROM deadline_completions c
                JOIN vehicle_deadlines d ON d.id = c.vehicle_deadline_id
                JOIN deadline_types t ON t.id = d.deadline_type_id
                WHERE NOT EXISTS (SELECT 1 FROM interventions i WHERE i.id = c.id);
                """);

            migrationBuilder.Sql("""
                INSERT INTO intervention_deadlines (intervention_id, vehicle_deadline_id)
                SELECT c.id, c.vehicle_deadline_id
                FROM deadline_completions c
                WHERE EXISTS (SELECT 1 FROM interventions i WHERE i.id = c.id)
                  AND NOT EXISTS (
                      SELECT 1 FROM intervention_deadlines l
                      WHERE l.intervention_id = c.id AND l.vehicle_deadline_id = c.vehicle_deadline_id);
                """);

            migrationBuilder.DropTable(
                name: "deadline_completions");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "deadline_completions",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    vehicle_deadline_id = table.Column<Guid>(type: "uuid", nullable: false),
                    date = table.Column<DateOnly>(type: "date", nullable: false),
                    kilometers = table.Column<int>(type: "integer", nullable: true),
                    comment = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    recorded_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_deadline_completions", x => x.id);
                    table.ForeignKey(
                        name: "fk_deadline_completions_vehicle_deadlines_vehicle_deadline_id",
                        column: x => x.vehicle_deadline_id,
                        principalTable: "vehicle_deadlines",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "ix_deadline_completions_vehicle_deadline_id",
                table: "deadline_completions",
                column: "vehicle_deadline_id");

            migrationBuilder.Sql("""
                INSERT INTO deadline_completions (id, vehicle_deadline_id, date, kilometers, comment, recorded_at)
                SELECT i.id,
                       l.vehicle_deadline_id,
                       i.date,
                       i.kilometers,
                       CASE WHEN i.work = t.name THEN NULL ELSE LEFT(i.work, 1000) END,
                       i.recorded_at
                FROM interventions i
                JOIN intervention_deadlines l ON l.intervention_id = i.id
                JOIN vehicle_deadlines d ON d.id = l.vehicle_deadline_id
                JOIN deadline_types t ON t.id = d.deadline_type_id
                WHERE (SELECT COUNT(*) FROM intervention_deadlines x WHERE x.intervention_id = i.id) = 1;
                """);
        }
    }
}
