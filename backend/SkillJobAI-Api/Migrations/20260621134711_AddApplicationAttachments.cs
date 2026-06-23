using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SkillJobAI_Api.Migrations
{
    /// <inheritdoc />
    public partial class AddApplicationAttachments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CertificateFileUrl",
                table: "Applications",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CvFileUrl",
                table: "Applications",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PortfolioFileUrl",
                table: "Applications",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CertificateFileUrl",
                table: "Applications");

            migrationBuilder.DropColumn(
                name: "CvFileUrl",
                table: "Applications");

            migrationBuilder.DropColumn(
                name: "PortfolioFileUrl",
                table: "Applications");
        }
    }
}
