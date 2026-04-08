using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Mss.WorkForce.Code.DataBaseManager.Migrations
{
    /// <inheritdoc />
    public partial class DeliveryIdInputOrders : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "DeliveryId",
                table: "InputOrders",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_InputOrders_DeliveryId",
                table: "InputOrders",
                column: "DeliveryId");

            migrationBuilder.AddForeignKey(
                name: "FK_InputOrders_Deliveries_DeliveryId",
                table: "InputOrders",
                column: "DeliveryId",
                principalTable: "Deliveries",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_InputOrders_Deliveries_DeliveryId",
                table: "InputOrders");

            migrationBuilder.DropIndex(
                name: "IX_InputOrders_DeliveryId",
                table: "InputOrders");

            migrationBuilder.DropColumn(
                name: "DeliveryId",
                table: "InputOrders");
        }
    }
}
