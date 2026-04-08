using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Mss.WorkForce.Code.DataBaseManager.Migrations
{
    /// <inheritdoc />
    public partial class FlowInProcess : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "FlowId",
                table: "Processes",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Processes_FlowId",
                table: "Processes",
                column: "FlowId");

            migrationBuilder.AddForeignKey(
                name: "FK_Processes_Flow_FlowId",
                table: "Processes",
                column: "FlowId",
                principalTable: "Flow",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Processes_Flow_FlowId",
                table: "Processes");

            migrationBuilder.DropIndex(
                name: "IX_Processes_FlowId",
                table: "Processes");

            migrationBuilder.DropColumn(
                name: "FlowId",
                table: "Processes");
        }
    }
}
