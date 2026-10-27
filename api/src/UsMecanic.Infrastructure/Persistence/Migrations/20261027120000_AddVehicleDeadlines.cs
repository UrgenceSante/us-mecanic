using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UsMecanic.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddVehicleDeadlines : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "vehicle_deadlines",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    vehicle_id = table.Column<Guid>(type: "uuid", nullable: false),
                    deadline_type_id = table.Column<Guid>(type: "uuid", nullable: false),
                    interval_months = table.Column<int>(type: "integer", nullable: true),
                    interval_km = table.Column<int>(type: "integer", nullable: true),
                    last_done_date = table.Column<DateOnly>(type: "date", nullable: true),
                    last_done_km = table.Column<int>(type: "integer", nullable: true),
                    due_date = table.Column<DateOnly>(type: "date", nullable: true),
                    due_km = table.Column<int>(type: "integer", nullable: true),
                    is_closed = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_vehicle_deadlines", x => x.id);
                    table.ForeignKey(
                        name: "fk_vehicle_deadlines_deadline_types_deadline_type_id",
                        column: x => x.deadline_type_id,
                        principalTable: "deadline_types",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_vehicle_deadlines_vehicles_vehicle_id",
                        column: x => x.vehicle_id,
                        principalTable: "vehicles",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "ix_vehicle_deadlines_deadline_type_id",
                table: "vehicle_deadlines",
                column: "deadline_type_id");

            migrationBuilder.CreateIndex(
                name: "ix_vehicle_deadlines_vehicle_id_deadline_type_id",
                table: "vehicle_deadlines",
                columns: new[] { "vehicle_id", "deadline_type_id" },
                unique: true,
                filter: "is_closed = false");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "vehicle_deadlines");
        }
    }
}
