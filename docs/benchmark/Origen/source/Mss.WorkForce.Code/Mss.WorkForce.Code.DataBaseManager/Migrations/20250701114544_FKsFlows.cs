using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Mss.WorkForce.Code.DataBaseManager.Migrations
{
    /// <inheritdoc />
    public partial class FKsFlows : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Processes_Flow_FlowId",
                table: "Processes");

            migrationBuilder.AddForeignKey(
                name: "FK_Processes_Flow_FlowId",
                table: "Processes",
                column: "FlowId",
                principalTable: "Flow",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Processes_Flow_FlowId",
                table: "Processes");

            migrationBuilder.AddForeignKey(
                name: "FK_Processes_Flow_FlowId",
                table: "Processes",
                column: "FlowId",
                principalTable: "Flow",
                principalColumn: "Id");
        }
    }
}
