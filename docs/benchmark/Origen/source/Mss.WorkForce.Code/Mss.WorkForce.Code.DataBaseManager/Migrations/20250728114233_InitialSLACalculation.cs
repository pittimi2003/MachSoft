using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Mss.WorkForce.Code.DataBaseManager.Migrations
{
    /// <inheritdoc />
    public partial class InitialSLACalculation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "OrderDelay",
                table: "WorkOrdersPlanning",
                type: "double precision",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "SLATarget",
                table: "WorkOrdersPlanning",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ShiftId",
                table: "ItemsPlanning",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ItemsPlanning_ShiftId",
                table: "ItemsPlanning",
                column: "ShiftId");

            migrationBuilder.AddForeignKey(
                name: "FK_ItemsPlanning_Shifts_ShiftId",
                table: "ItemsPlanning",
                column: "ShiftId",
                principalTable: "Shifts",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ItemsPlanning_Shifts_ShiftId",
                table: "ItemsPlanning");

            migrationBuilder.DropIndex(
                name: "IX_ItemsPlanning_ShiftId",
                table: "ItemsPlanning");

            migrationBuilder.DropColumn(
                name: "OrderDelay",
                table: "WorkOrdersPlanning");

            migrationBuilder.DropColumn(
                name: "SLATarget",
                table: "WorkOrdersPlanning");

            migrationBuilder.DropColumn(
                name: "ShiftId",
                table: "ItemsPlanning");
        }
    }
}
