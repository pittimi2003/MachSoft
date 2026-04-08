using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Mss.WorkForce.Code.DataBaseManager.Migrations
{
    /// <inheritdoc />
    public partial class StageItemPlanning : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "StageId",
                table: "ItemsPlanning",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ItemsPlanning_StageId",
                table: "ItemsPlanning",
                column: "StageId");

            migrationBuilder.AddForeignKey(
                name: "FK_ItemsPlanning_Stages_StageId",
                table: "ItemsPlanning",
                column: "StageId",
                principalTable: "Stages",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ItemsPlanning_Stages_StageId",
                table: "ItemsPlanning");

            migrationBuilder.DropIndex(
                name: "IX_ItemsPlanning_StageId",
                table: "ItemsPlanning");

            migrationBuilder.DropColumn(
                name: "StageId",
                table: "ItemsPlanning");
        }
    }
}
