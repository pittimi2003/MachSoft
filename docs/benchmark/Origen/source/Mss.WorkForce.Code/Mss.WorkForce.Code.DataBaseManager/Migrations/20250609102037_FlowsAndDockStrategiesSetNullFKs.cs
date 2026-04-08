using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Mss.WorkForce.Code.DataBaseManager.Migrations
{
    /// <inheritdoc />
    public partial class FlowsAndDockStrategiesSetNullFKs : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_InboundFlowGraphs_DockSelectionStrategies_DockSelectionStra~",
                table: "InboundFlowGraphs");

            migrationBuilder.DropForeignKey(
                name: "FK_OutboundFlowGraphs_DockSelectionStrategies_DockSelectionStr~",
                table: "OutboundFlowGraphs");

            migrationBuilder.AddForeignKey(
                name: "FK_InboundFlowGraphs_DockSelectionStrategies_DockSelectionStra~",
                table: "InboundFlowGraphs",
                column: "DockSelectionStrategyId",
                principalTable: "DockSelectionStrategies",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_OutboundFlowGraphs_DockSelectionStrategies_DockSelectionStr~",
                table: "OutboundFlowGraphs",
                column: "DockSelectionStrategyId",
                principalTable: "DockSelectionStrategies",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_InboundFlowGraphs_DockSelectionStrategies_DockSelectionStra~",
                table: "InboundFlowGraphs");

            migrationBuilder.DropForeignKey(
                name: "FK_OutboundFlowGraphs_DockSelectionStrategies_DockSelectionStr~",
                table: "OutboundFlowGraphs");

            migrationBuilder.AddForeignKey(
                name: "FK_InboundFlowGraphs_DockSelectionStrategies_DockSelectionStra~",
                table: "InboundFlowGraphs",
                column: "DockSelectionStrategyId",
                principalTable: "DockSelectionStrategies",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_OutboundFlowGraphs_DockSelectionStrategies_DockSelectionStr~",
                table: "OutboundFlowGraphs",
                column: "DockSelectionStrategyId",
                principalTable: "DockSelectionStrategies",
                principalColumn: "Id");
        }
    }
}
