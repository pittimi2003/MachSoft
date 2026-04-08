using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Mss.WorkForce.Code.DataBaseManager.Migrations
{
    /// <inheritdoc />
    public partial class LanguageAndDateFormatFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "InternationalCode",
                table: "Languages",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DateTimeFormat",
                table: "DateFormats",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "InternationalCode",
                table: "Languages");

            migrationBuilder.DropColumn(
                name: "DateTimeFormat",
                table: "DateFormats");
        }
    }
}
