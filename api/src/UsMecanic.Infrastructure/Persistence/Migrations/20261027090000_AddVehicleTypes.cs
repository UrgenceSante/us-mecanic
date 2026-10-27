using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UsMecanic.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddVehicleTypes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "type_id",
                table: "vehicles",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "vehicle_types",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_vehicle_types", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "ix_vehicles_type_id",
                table: "vehicles",
                column: "type_id");

            migrationBuilder.CreateIndex(
                name: "ix_vehicle_types_name",
                table: "vehicle_types",
                column: "name",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "fk_vehicles_vehicle_types_type_id",
                table: "vehicles",
                column: "type_id",
                principalTable: "vehicle_types",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_vehicles_vehicle_types_type_id",
                table: "vehicles");

            migrationBuilder.DropTable(
                name: "vehicle_types");

            migrationBuilder.DropIndex(
                name: "ix_vehicles_type_id",
                table: "vehicles");

            migrationBuilder.DropColumn(
                name: "type_id",
                table: "vehicles");
        }
    }
}
