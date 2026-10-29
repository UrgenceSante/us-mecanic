using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UsMecanic.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddProblemAnalysis : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "analysis_text",
                table: "problems",
                type: "character varying(4000)",
                maxLength: 4000,
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "analyzed_at",
                table: "problems",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "element_id",
                table: "problems",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "immobilized",
                table: "problems",
                type: "boolean",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "nature_id",
                table: "problems",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "ix_problems_element_id",
                table: "problems",
                column: "element_id");

            migrationBuilder.CreateIndex(
                name: "ix_problems_nature_id",
                table: "problems",
                column: "nature_id");

            migrationBuilder.AddForeignKey(
                name: "fk_problems_problem_elements_element_id",
                table: "problems",
                column: "element_id",
                principalTable: "problem_elements",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "fk_problems_problem_natures_nature_id",
                table: "problems",
                column: "nature_id",
                principalTable: "problem_natures",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_problems_problem_elements_element_id",
                table: "problems");

            migrationBuilder.DropForeignKey(
                name: "fk_problems_problem_natures_nature_id",
                table: "problems");

            migrationBuilder.DropIndex(
                name: "ix_problems_element_id",
                table: "problems");

            migrationBuilder.DropIndex(
                name: "ix_problems_nature_id",
                table: "problems");

            migrationBuilder.DropColumn(
                name: "analysis_text",
                table: "problems");

            migrationBuilder.DropColumn(
                name: "analyzed_at",
                table: "problems");

            migrationBuilder.DropColumn(
                name: "element_id",
                table: "problems");

            migrationBuilder.DropColumn(
                name: "immobilized",
                table: "problems");

            migrationBuilder.DropColumn(
                name: "nature_id",
                table: "problems");
        }
    }
}
