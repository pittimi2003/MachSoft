using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Mss.WorkForce.Code.DataBaseManager.Migrations
{
    /// <inheritdoc />
    public partial class HourFormat : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "HourFormatId",
                table: "Users",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "HourFormatId",
                table: "Organizations",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "HourFormats",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    HourTimeFormat = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HourFormats", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Users_HourFormatId",
                table: "Users",
                column: "HourFormatId");

            migrationBuilder.CreateIndex(
                name: "IX_Organizations_HourFormatId",
                table: "Organizations",
                column: "HourFormatId");

            migrationBuilder.AddForeignKey(
                name: "FK_Organizations_HourFormats_HourFormatId",
                table: "Organizations",
                column: "HourFormatId",
                principalTable: "HourFormats",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Users_HourFormats_HourFormatId",
                table: "Users",
                column: "HourFormatId",
                principalTable: "HourFormats",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Organizations_HourFormats_HourFormatId",
                table: "Organizations");

            migrationBuilder.DropForeignKey(
                name: "FK_Users_HourFormats_HourFormatId",
                table: "Users");

            migrationBuilder.DropTable(
                name: "HourFormats");

            migrationBuilder.DropIndex(
                name: "IX_Users_HourFormatId",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Organizations_HourFormatId",
                table: "Organizations");

            migrationBuilder.DropColumn(
                name: "HourFormatId",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "HourFormatId",
                table: "Organizations");
        }
    }
}
