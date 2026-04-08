using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Mss.WorkForce.Code.DataBaseManager.Migrations
{
    /// <inheritdoc />
    public partial class CustomFlow : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "FlowId",
                table: "OutboundFlowGraphs",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "FlowId",
                table: "InboundFlowGraphs",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Flow",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Type = table.Column<string>(type: "text", nullable: false),
                    WarehouseId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Flow", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Flow_Warehouses_WarehouseId",
                        column: x => x.WarehouseId,
                        principalTable: "Warehouses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CustomFlowGraphs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    FlowId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CustomFlowGraphs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CustomFlowGraphs_Flow_FlowId",
                        column: x => x.FlowId,
                        principalTable: "Flow",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_OutboundFlowGraphs_FlowId",
                table: "OutboundFlowGraphs",
                column: "FlowId");

            migrationBuilder.CreateIndex(
                name: "IX_InboundFlowGraphs_FlowId",
                table: "InboundFlowGraphs",
                column: "FlowId");

            migrationBuilder.CreateIndex(
                name: "IX_CustomFlowGraphs_FlowId",
                table: "CustomFlowGraphs",
                column: "FlowId");

            migrationBuilder.CreateIndex(
                name: "IX_Flow_WarehouseId",
                table: "Flow",
                column: "WarehouseId");

            migrationBuilder.AddForeignKey(
                name: "FK_InboundFlowGraphs_Flow_FlowId",
                table: "InboundFlowGraphs",
                column: "FlowId",
                principalTable: "Flow",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_OutboundFlowGraphs_Flow_FlowId",
                table: "OutboundFlowGraphs",
                column: "FlowId",
                principalTable: "Flow",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_InboundFlowGraphs_Flow_FlowId",
                table: "InboundFlowGraphs");

            migrationBuilder.DropForeignKey(
                name: "FK_OutboundFlowGraphs_Flow_FlowId",
                table: "OutboundFlowGraphs");

            migrationBuilder.DropTable(
                name: "CustomFlowGraphs");

            migrationBuilder.DropTable(
                name: "Flow");

            migrationBuilder.DropIndex(
                name: "IX_OutboundFlowGraphs_FlowId",
                table: "OutboundFlowGraphs");

            migrationBuilder.DropIndex(
                name: "IX_InboundFlowGraphs_FlowId",
                table: "InboundFlowGraphs");

            migrationBuilder.DropColumn(
                name: "FlowId",
                table: "OutboundFlowGraphs");

            migrationBuilder.DropColumn(
                name: "FlowId",
                table: "InboundFlowGraphs");
        }
    }
}
