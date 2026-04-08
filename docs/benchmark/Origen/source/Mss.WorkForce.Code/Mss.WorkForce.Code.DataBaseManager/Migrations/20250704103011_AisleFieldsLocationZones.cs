using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Mss.WorkForce.Code.DataBaseManager.Migrations
{
    /// <inheritdoc />
    public partial class AisleFieldsLocationZones : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "Bidirectional",
                table: "Racks",
                type: "boolean",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "MaxPickers",
                table: "Racks",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "NarrowAisle",
                table: "Racks",
                type: "boolean",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "Bidirectional",
                table: "DriveIns",
                type: "boolean",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "MaxPickers",
                table: "DriveIns",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "NarrowAisle",
                table: "DriveIns",
                type: "boolean",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Bidirectional",
                table: "Racks");

            migrationBuilder.DropColumn(
                name: "MaxPickers",
                table: "Racks");

            migrationBuilder.DropColumn(
                name: "NarrowAisle",
                table: "Racks");

            migrationBuilder.DropColumn(
                name: "Bidirectional",
                table: "DriveIns");

            migrationBuilder.DropColumn(
                name: "MaxPickers",
                table: "DriveIns");

            migrationBuilder.DropColumn(
                name: "NarrowAisle",
                table: "DriveIns");
        }
    }
}
