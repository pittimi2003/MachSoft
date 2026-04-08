using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Mss.WorkForce.Code.DataBaseManager.Migrations
{
    /// <inheritdoc />
    public partial class FixDeleteYardUsage : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_YardUsagePerHour_Docks_DockId",
                table: "YardUsagePerHour");

            migrationBuilder.DropForeignKey(
                name: "FK_YardUsagePerHour_Plannings_PlanningId",
                table: "YardUsagePerHour");

            migrationBuilder.DropForeignKey(
                name: "FK_YardUsagePerHour_Warehouses_WarehouseId",
                table: "YardUsagePerHour");

            migrationBuilder.AddForeignKey(
                name: "FK_YardUsagePerHour_Docks_DockId",
                table: "YardUsagePerHour",
                column: "DockId",
                principalTable: "Docks",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_YardUsagePerHour_Plannings_PlanningId",
                table: "YardUsagePerHour",
                column: "PlanningId",
                principalTable: "Plannings",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_YardUsagePerHour_Warehouses_WarehouseId",
                table: "YardUsagePerHour",
                column: "WarehouseId",
                principalTable: "Warehouses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_YardUsagePerHour_Docks_DockId",
                table: "YardUsagePerHour");

            migrationBuilder.DropForeignKey(
                name: "FK_YardUsagePerHour_Plannings_PlanningId",
                table: "YardUsagePerHour");

            migrationBuilder.DropForeignKey(
                name: "FK_YardUsagePerHour_Warehouses_WarehouseId",
                table: "YardUsagePerHour");

            migrationBuilder.AddForeignKey(
                name: "FK_YardUsagePerHour_Docks_DockId",
                table: "YardUsagePerHour",
                column: "DockId",
                principalTable: "Docks",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_YardUsagePerHour_Plannings_PlanningId",
                table: "YardUsagePerHour",
                column: "PlanningId",
                principalTable: "Plannings",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_YardUsagePerHour_Warehouses_WarehouseId",
                table: "YardUsagePerHour",
                column: "WarehouseId",
                principalTable: "Warehouses",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }
    }
}
