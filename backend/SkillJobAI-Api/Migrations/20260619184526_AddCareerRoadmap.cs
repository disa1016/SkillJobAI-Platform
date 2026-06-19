using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace SkillJobAI_Api.Migrations
{
    /// <inheritdoc />
    public partial class AddCareerRoadmap : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CareerGoals",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false),
                    DurationMonths = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CareerGoals", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CareerGoalSkills",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CareerGoalId = table.Column<int>(type: "integer", nullable: false),
                    SkillId = table.Column<int>(type: "integer", nullable: false),
                    MonthNumber = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CareerGoalSkills", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CareerGoalSkills_CareerGoals_CareerGoalId",
                        column: x => x.CareerGoalId,
                        principalTable: "CareerGoals",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CareerGoalSkills_Skills_SkillId",
                        column: x => x.SkillId,
                        principalTable: "Skills",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserCareerGoals",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    CareerGoalId = table.Column<int>(type: "integer", nullable: false),
                    StartedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserCareerGoals", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserCareerGoals_CareerGoals_CareerGoalId",
                        column: x => x.CareerGoalId,
                        principalTable: "CareerGoals",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserCareerGoals_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CareerGoalSkills_CareerGoalId",
                table: "CareerGoalSkills",
                column: "CareerGoalId");

            migrationBuilder.CreateIndex(
                name: "IX_CareerGoalSkills_SkillId",
                table: "CareerGoalSkills",
                column: "SkillId");

            migrationBuilder.CreateIndex(
                name: "IX_UserCareerGoals_CareerGoalId",
                table: "UserCareerGoals",
                column: "CareerGoalId");

            migrationBuilder.CreateIndex(
                name: "IX_UserCareerGoals_UserId",
                table: "UserCareerGoals",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CareerGoalSkills");

            migrationBuilder.DropTable(
                name: "UserCareerGoals");

            migrationBuilder.DropTable(
                name: "CareerGoals");
        }
    }
}
