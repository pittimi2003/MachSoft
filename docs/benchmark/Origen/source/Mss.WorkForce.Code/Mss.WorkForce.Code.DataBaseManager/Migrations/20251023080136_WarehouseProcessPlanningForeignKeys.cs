using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Mss.WorkForce.Code.DataBaseManager.Migrations
{
    /// <inheritdoc />
    public partial class WarehouseProcessPlanningForeignKeys : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_WarehouseProcessPlanning_EquipmentGroups_EquipmentGroupId",
                table: "WarehouseProcessPlanning");

            migrationBuilder.DropForeignKey(
                name: "FK_WarehouseProcessPlanning_Workers_WorkerId",
                table: "WarehouseProcessPlanning");

            migrationBuilder.AddForeignKey(
                name: "FK_WarehouseProcessPlanning_EquipmentGroups_EquipmentGroupId",
                table: "WarehouseProcessPlanning",
                column: "EquipmentGroupId",
                principalTable: "EquipmentGroups",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_WarehouseProcessPlanning_Workers_WorkerId",
                table: "WarehouseProcessPlanning",
                column: "WorkerId",
                principalTable: "Workers",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_WarehouseProcessPlanning_EquipmentGroups_EquipmentGroupId",
                table: "WarehouseProcessPlanning");

            migrationBuilder.DropForeignKey(
                name: "FK_WarehouseProcessPlanning_Workers_WorkerId",
                table: "WarehouseProcessPlanning");

            migrationBuilder.AddForeignKey(
                name: "FK_WarehouseProcessPlanning_EquipmentGroups_EquipmentGroupId",
                table: "WarehouseProcessPlanning",
                column: "EquipmentGroupId",
                principalTable: "EquipmentGroups",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_WarehouseProcessPlanning_Workers_WorkerId",
                table: "WarehouseProcessPlanning",
                column: "WorkerId",
                principalTable: "Workers",
                principalColumn: "Id");
        }
    }
}
