using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Mss.WorkForce.Code.DataBaseManager.Migrations
{
    /// <inheritdoc />
    public partial class UpdateDateWMS : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "UpdateDateWMS",
                table: "InputOrders",
                type: "timestamp with time zone",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UpdateDateWMS",
                table: "InputOrders");
        }
    }
}
