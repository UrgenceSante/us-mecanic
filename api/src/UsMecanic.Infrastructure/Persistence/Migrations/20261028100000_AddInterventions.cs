using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UsMecanic.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddInterventions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "interventions",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    vehicle_id = table.Column<Guid>(type: "uuid", nullable: false),
                    date = table.Column<DateOnly>(type: "date", nullable: false),
                    kilometers = table.Column<int>(type: "integer", nullable: true),
                    garage_id = table.Column<Guid>(type: "uuid", nullable: true),
                    work = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    recorded_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_interventions", x => x.id);
                    table.ForeignKey(
                        name: "fk_interventions_garages_garage_id",
                        column: x => x.garage_id,
                        principalTable: "garages",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_interventions_vehicles_vehicle_id",
                        column: x => x.vehicle_id,
                        principalTable: "vehicles",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "intervention_deadlines",
                columns: table => new
                {
                    intervention_id = table.Column<Guid>(type: "uuid", nullable: false),
                    vehicle_deadline_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_intervention_deadlines", x => new { x.intervention_id, x.vehicle_deadline_id });
                    table.ForeignKey(
                        name: "fk_intervention_deadlines_interventions_intervention_id",
                        column: x => x.intervention_id,
                        principalTable: "interventions",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_intervention_deadlines_vehicle_deadlines_vehicle_deadline_id",
                        column: x => x.vehicle_deadline_id,
                        principalTable: "vehicle_deadlines",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "ix_intervention_deadlines_vehicle_deadline_id",
                table: "intervention_deadlines",
                column: "vehicle_deadline_id");

            migrationBuilder.CreateIndex(
                name: "ix_interventions_garage_id",
                table: "interventions",
                column: "garage_id");

            migrationBuilder.CreateIndex(
                name: "ix_interventions_vehicle_id_date",
                table: "interventions",
                columns: new[] { "vehicle_id", "date" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "intervention_deadlines");

            migrationBuilder.DropTable(
                name: "interventions");
        }
    }
}
