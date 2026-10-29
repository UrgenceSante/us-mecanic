using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UsMecanic.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddProblemActions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "problem_actions",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    problem_id = table.Column<Guid>(type: "uuid", nullable: false),
                    actor_id = table.Column<Guid>(type: "uuid", nullable: false),
                    type_id = table.Column<Guid>(type: "uuid", nullable: false),
                    constraint_id = table.Column<Guid>(type: "uuid", nullable: false),
                    due_date = table.Column<DateOnly>(type: "date", nullable: true),
                    comment = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    intervention_id = table.Column<Guid>(type: "uuid", nullable: true),
                    recorded_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    closed_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_problem_actions", x => x.id);
                    table.ForeignKey(
                        name: "fk_problem_actions_interventions_intervention_id",
                        column: x => x.intervention_id,
                        principalTable: "interventions",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "fk_problem_actions_problem_action_types_type_id",
                        column: x => x.type_id,
                        principalTable: "problem_action_types",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_problem_actions_problem_actors_actor_id",
                        column: x => x.actor_id,
                        principalTable: "problem_actors",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_problem_actions_problem_constraints_constraint_id",
                        column: x => x.constraint_id,
                        principalTable: "problem_constraints",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_problem_actions_problems_problem_id",
                        column: x => x.problem_id,
                        principalTable: "problems",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "ix_problem_actions_actor_id",
                table: "problem_actions",
                column: "actor_id");

            migrationBuilder.CreateIndex(
                name: "ix_problem_actions_constraint_id",
                table: "problem_actions",
                column: "constraint_id");

            migrationBuilder.CreateIndex(
                name: "ix_problem_actions_intervention_id",
                table: "problem_actions",
                column: "intervention_id");

            migrationBuilder.CreateIndex(
                name: "ix_problem_actions_problem_id",
                table: "problem_actions",
                column: "problem_id");

            migrationBuilder.CreateIndex(
                name: "ix_problem_actions_type_id",
                table: "problem_actions",
                column: "type_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "problem_actions");
        }
    }
}
