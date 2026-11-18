using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UsMecanic.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddVehicleOwner : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "owner_company_id",
                table: "vehicles",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "ix_vehicles_owner_company_id",
                table: "vehicles",
                column: "owner_company_id");

            migrationBuilder.AddForeignKey(
                name: "fk_vehicles_companies_owner_company_id",
                table: "vehicles",
                column: "owner_company_id",
                principalTable: "companies",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_vehicles_companies_owner_company_id",
                table: "vehicles");

            migrationBuilder.DropIndex(
                name: "ix_vehicles_owner_company_id",
                table: "vehicles");

            migrationBuilder.DropColumn(
                name: "owner_company_id",
                table: "vehicles");
        }
    }
}
