using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SkillJobAI_Api.Migrations
{
    /// <inheritdoc />
    public partial class AddCvUrlToUsers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CvUrl",
                table: "Users",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CvUrl",
                table: "Users");
        }
    }
}
