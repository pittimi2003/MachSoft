using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Mss.WorkForce.Code.DataBaseManager.Migrations
{
    /// <inheritdoc />
    public partial class LaborProgress : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "Progress",
                table: "WFMLaborWorkerPerProcessType",
                type: "double precision",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "Progress",
                table: "WFMLaborWorkerPerFlow",
                type: "double precision",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "Progress",
                table: "WFMLaborWorker",
                type: "double precision",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "Progress",
                table: "WFMLaborItemPlanning",
                type: "double precision",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "Progress",
                table: "WFMLaborEquipmentPerProcessType",
                type: "double precision",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "Progress",
                table: "WFMLaborEquipmentPerFlow",
                type: "double precision",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "Progress",
                table: "WFMLaborEquipment",
                type: "double precision",
                nullable: false,
                defaultValue: 0.0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Progress",
                table: "WFMLaborWorkerPerProcessType");

            migrationBuilder.DropColumn(
                name: "Progress",
                table: "WFMLaborWorkerPerFlow");

            migrationBuilder.DropColumn(
                name: "Progress",
                table: "WFMLaborWorker");

            migrationBuilder.DropColumn(
                name: "Progress",
                table: "WFMLaborItemPlanning");

            migrationBuilder.DropColumn(
                name: "Progress",
                table: "WFMLaborEquipmentPerProcessType");

            migrationBuilder.DropColumn(
                name: "Progress",
                table: "WFMLaborEquipmentPerFlow");

            migrationBuilder.DropColumn(
                name: "Progress",
                table: "WFMLaborEquipment");
        }
    }
}
