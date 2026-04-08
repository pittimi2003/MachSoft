using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Mss.WorkForce.Code.DataBaseManager.Migrations
{
    /// <inheritdoc />
    public partial class YardUsageAndSaturation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "YardUsagePerHour",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    InitHour = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EndHour = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DockId = table.Column<Guid>(type: "uuid", nullable: false),
                    AllowInbound = table.Column<bool>(type: "boolean", nullable: false),
                    AllowOutbound = table.Column<bool>(type: "boolean", nullable: false),
                    TotalCapacity = table.Column<double>(type: "double precision", nullable: false),
                    RealUsage = table.Column<double>(type: "double precision", nullable: false),
                    PlannedUsage = table.Column<double>(type: "double precision", nullable: false),
                    Saturation = table.Column<double>(type: "double precision", nullable: false),
                    PlanningId = table.Column<Guid>(type: "uuid", nullable: false),
                    WarehouseId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_YardUsagePerHour", x => x.Id);
                    table.ForeignKey(
                        name: "FK_YardUsagePerHour_Docks_DockId",
                        column: x => x.DockId,
                        principalTable: "Docks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_YardUsagePerHour_Plannings_PlanningId",
                        column: x => x.PlanningId,
                        principalTable: "Plannings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_YardUsagePerHour_Warehouses_WarehouseId",
                        column: x => x.WarehouseId,
                        principalTable: "Warehouses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_YardUsagePerHour_DockId",
                table: "YardUsagePerHour",
                column: "DockId");

            migrationBuilder.CreateIndex(
                name: "IX_YardUsagePerHour_PlanningId",
                table: "YardUsagePerHour",
                column: "PlanningId");

            migrationBuilder.CreateIndex(
                name: "IX_YardUsagePerHour_WarehouseId",
                table: "YardUsagePerHour",
                column: "WarehouseId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "YardUsagePerHour");
        }
    }
}
