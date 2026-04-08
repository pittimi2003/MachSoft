using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Mss.WorkForce.Code.DataBaseManager.Migrations
{
    /// <inheritdoc />
    public partial class SetNullShiftItemPlanning : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ItemsPlanning_Shifts_ShiftId",
                table: "ItemsPlanning");

            migrationBuilder.AddForeignKey(
                name: "FK_ItemsPlanning_Shifts_ShiftId",
                table: "ItemsPlanning",
                column: "ShiftId",
                principalTable: "Shifts",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ItemsPlanning_Shifts_ShiftId",
                table: "ItemsPlanning");

            migrationBuilder.AddForeignKey(
                name: "FK_ItemsPlanning_Shifts_ShiftId",
                table: "ItemsPlanning",
                column: "ShiftId",
                principalTable: "Shifts",
                principalColumn: "Id");
        }
    }
}
