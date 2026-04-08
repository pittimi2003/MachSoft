using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Mss.WorkForce.Code.DataBaseManager.Migrations
{
    /// <inheritdoc />
    public partial class TemporalyBreakAndUpdateShift : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "CustomEndHour",
                table: "Schedules",
                type: "double precision",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "CustomInitHour",
                table: "Schedules",
                type: "double precision",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CustomEndHour",
                table: "Schedules");

            migrationBuilder.DropColumn(
                name: "CustomInitHour",
                table: "Schedules");
        }
    }
}
