using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UsMecanic.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class UpdateMaintenanceDefaultInterval : Migration
    {
        private static readonly Guid MaintenanceTypeId = new("0d1a5e00-0000-4000-8000-000000000012");

        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // « Entretien constructeur » : tous les 12 mois ou 30 000 km.
            migrationBuilder.UpdateData(
                table: "deadline_types",
                keyColumn: "id",
                keyValue: MaintenanceTypeId,
                columns: ["default_interval_months", "default_interval_km"],
                values: [12, 30000]);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "deadline_types",
                keyColumn: "id",
                keyValue: MaintenanceTypeId,
                columns: ["default_interval_months", "default_interval_km"],
                values: [null, null]);
        }
    }
}
