using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Mss.WorkForce.Code.DataBaseManager.Migrations
{
    /// <inheritdoc />
    public partial class MaxEquipmentsToZones : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "MaxEquipments",
                table: "Racks",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "MaxEquipments",
                table: "DriveIns",
                type: "integer",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MaxEquipments",
                table: "Racks");

            migrationBuilder.DropColumn(
                name: "MaxEquipments",
                table: "DriveIns");
        }
    }
}
