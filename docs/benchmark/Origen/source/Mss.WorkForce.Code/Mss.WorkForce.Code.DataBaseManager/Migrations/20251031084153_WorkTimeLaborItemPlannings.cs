using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Mss.WorkForce.Code.DataBaseManager.Migrations
{
    /// <inheritdoc />
    public partial class WorkTimeLaborItemPlannings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "WorkTime",
                table: "WFMLaborItemPlanning",
                type: "double precision",
                nullable: false,
                defaultValue: 0.0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "WorkTime",
                table: "WFMLaborItemPlanning");
        }
    }
}
