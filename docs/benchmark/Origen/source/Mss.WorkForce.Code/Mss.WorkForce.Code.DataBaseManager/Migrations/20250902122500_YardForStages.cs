using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Mss.WorkForce.Code.DataBaseManager.Migrations
{
    /// <inheritdoc />
    public partial class YardForStages : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "YardUsagePerHour");

            migrationBuilder.AddColumn<Guid>(
                name: "YardMetricsPerStageId",
                table: "YardMetricsAppointments",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "YardDockUsagePerHour",
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
                    table.PrimaryKey("PK_YardDockUsagePerHour", x => x.Id);
                    table.ForeignKey(
                        name: "FK_YardDockUsagePerHour_Docks_DockId",
                        column: x => x.DockId,
                        principalTable: "Docks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_YardDockUsagePerHour_Plannings_PlanningId",
                        column: x => x.PlanningId,
                        principalTable: "Plannings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_YardDockUsagePerHour_Warehouses_WarehouseId",
                        column: x => x.WarehouseId,
                        principalTable: "Warehouses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "YardMetricsPerStage",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ProcessType = table.Column<string>(type: "text", nullable: false),
                    StartDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EndDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    StageId = table.Column<Guid>(type: "uuid", nullable: false),
                    AttendedAppointments = table.Column<int>(type: "integer", nullable: false),
                    TotalAppointments = table.Column<int>(type: "integer", nullable: false),
                    Saturation = table.Column<double>(type: "double precision", nullable: false),
                    PlanningId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_YardMetricsPerStage", x => x.Id);
                    table.ForeignKey(
                        name: "FK_YardMetricsPerStage_Plannings_PlanningId",
                        column: x => x.PlanningId,
                        principalTable: "Plannings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_YardMetricsPerStage_Stages_StageId",
                        column: x => x.StageId,
                        principalTable: "Stages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "YardStageUsagePerHour",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    InitHour = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EndHour = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    StageId = table.Column<Guid>(type: "uuid", nullable: false),
                    TotalCapacity = table.Column<double>(type: "double precision", nullable: false),
                    RealUsage = table.Column<double>(type: "double precision", nullable: false),
                    PlannedUsage = table.Column<double>(type: "double precision", nullable: false),
                    Saturation = table.Column<double>(type: "double precision", nullable: false),
                    PlanningId = table.Column<Guid>(type: "uuid", nullable: false),
                    WarehouseId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_YardStageUsagePerHour", x => x.Id);
                    table.ForeignKey(
                        name: "FK_YardStageUsagePerHour_Plannings_PlanningId",
                        column: x => x.PlanningId,
                        principalTable: "Plannings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_YardStageUsagePerHour_Stages_StageId",
                        column: x => x.StageId,
                        principalTable: "Stages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_YardStageUsagePerHour_Warehouses_WarehouseId",
                        column: x => x.WarehouseId,
                        principalTable: "Warehouses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_YardMetricsAppointments_YardMetricsPerStageId",
                table: "YardMetricsAppointments",
                column: "YardMetricsPerStageId");

            migrationBuilder.CreateIndex(
                name: "IX_YardDockUsagePerHour_DockId",
                table: "YardDockUsagePerHour",
                column: "DockId");

            migrationBuilder.CreateIndex(
                name: "IX_YardDockUsagePerHour_PlanningId",
                table: "YardDockUsagePerHour",
                column: "PlanningId");

            migrationBuilder.CreateIndex(
                name: "IX_YardDockUsagePerHour_WarehouseId",
                table: "YardDockUsagePerHour",
                column: "WarehouseId");

            migrationBuilder.CreateIndex(
                name: "IX_YardMetricsPerStage_PlanningId",
                table: "YardMetricsPerStage",
                column: "PlanningId");

            migrationBuilder.CreateIndex(
                name: "IX_YardMetricsPerStage_StageId",
                table: "YardMetricsPerStage",
                column: "StageId");

            migrationBuilder.CreateIndex(
                name: "IX_YardStageUsagePerHour_PlanningId",
                table: "YardStageUsagePerHour",
                column: "PlanningId");

            migrationBuilder.CreateIndex(
                name: "IX_YardStageUsagePerHour_StageId",
                table: "YardStageUsagePerHour",
                column: "StageId");

            migrationBuilder.CreateIndex(
                name: "IX_YardStageUsagePerHour_WarehouseId",
                table: "YardStageUsagePerHour",
                column: "WarehouseId");

            migrationBuilder.AddForeignKey(
                name: "FK_YardMetricsAppointments_YardMetricsPerStage_YardMetricsPerS~",
                table: "YardMetricsAppointments",
                column: "YardMetricsPerStageId",
                principalTable: "YardMetricsPerStage",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_YardMetricsAppointments_YardMetricsPerStage_YardMetricsPerS~",
                table: "YardMetricsAppointments");

            migrationBuilder.DropTable(
                name: "YardDockUsagePerHour");

            migrationBuilder.DropTable(
                name: "YardMetricsPerStage");

            migrationBuilder.DropTable(
                name: "YardStageUsagePerHour");

            migrationBuilder.DropIndex(
                name: "IX_YardMetricsAppointments_YardMetricsPerStageId",
                table: "YardMetricsAppointments");

            migrationBuilder.DropColumn(
                name: "YardMetricsPerStageId",
                table: "YardMetricsAppointments");

            migrationBuilder.CreateTable(
                name: "YardUsagePerHour",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    DockId = table.Column<Guid>(type: "uuid", nullable: false),
                    PlanningId = table.Column<Guid>(type: "uuid", nullable: false),
                    WarehouseId = table.Column<Guid>(type: "uuid", nullable: false),
                    AllowInbound = table.Column<bool>(type: "boolean", nullable: false),
                    AllowOutbound = table.Column<bool>(type: "boolean", nullable: false),
                    EndHour = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    InitHour = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    PlannedUsage = table.Column<double>(type: "double precision", nullable: false),
                    RealUsage = table.Column<double>(type: "double precision", nullable: false),
                    Saturation = table.Column<double>(type: "double precision", nullable: false),
                    TotalCapacity = table.Column<double>(type: "double precision", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_YardUsagePerHour", x => x.Id);
                    table.ForeignKey(
                        name: "FK_YardUsagePerHour_Docks_DockId",
                        column: x => x.DockId,
                        principalTable: "Docks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_YardUsagePerHour_Plannings_PlanningId",
                        column: x => x.PlanningId,
                        principalTable: "Plannings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_YardUsagePerHour_Warehouses_WarehouseId",
                        column: x => x.WarehouseId,
                        principalTable: "Warehouses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
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
    }
}
