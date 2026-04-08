using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Mss.WorkForce.Code.DataBaseManager.Migrations
{
    /// <inheritdoc />
    public partial class ChangeYardAppointmentMetrics : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "YardMetricsAppointments");

            migrationBuilder.CreateTable(
                name: "YardMetricsAppointmentsPerDock",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AppointmentCode = table.Column<string>(type: "text", nullable: false),
                    ProcessType = table.Column<string>(type: "text", nullable: false),
                    AppointmentDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    StartDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EndDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Customer = table.Column<string>(type: "text", nullable: false),
                    YardCode = table.Column<string>(type: "text", nullable: false),
                    DockId = table.Column<Guid>(type: "uuid", nullable: false),
                    VehicleType = table.Column<string>(type: "text", nullable: false),
                    License = table.Column<string>(type: "text", nullable: false),
                    Progress = table.Column<double>(type: "double precision", nullable: false),
                    TotalOrders = table.Column<int>(type: "integer", nullable: false),
                    CompletedOrders = table.Column<int>(type: "integer", nullable: false),
                    PlanningId = table.Column<Guid>(type: "uuid", nullable: false),
                    YardMetricsPerDockId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_YardMetricsAppointmentsPerDock", x => x.Id);
                    table.ForeignKey(
                        name: "FK_YardMetricsAppointmentsPerDock_Docks_DockId",
                        column: x => x.DockId,
                        principalTable: "Docks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_YardMetricsAppointmentsPerDock_Plannings_PlanningId",
                        column: x => x.PlanningId,
                        principalTable: "Plannings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_YardMetricsAppointmentsPerDock_YardMetricsPerDock_YardMetri~",
                        column: x => x.YardMetricsPerDockId,
                        principalTable: "YardMetricsPerDock",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "YardMetricsAppointmentsPerStage",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AppointmentCode = table.Column<string>(type: "text", nullable: false),
                    ProcessType = table.Column<string>(type: "text", nullable: false),
                    AppointmentDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    StartDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EndDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Customer = table.Column<string>(type: "text", nullable: false),
                    YardCode = table.Column<string>(type: "text", nullable: false),
                    StageId = table.Column<Guid>(type: "uuid", nullable: false),
                    VehicleType = table.Column<string>(type: "text", nullable: false),
                    License = table.Column<string>(type: "text", nullable: false),
                    Progress = table.Column<double>(type: "double precision", nullable: false),
                    TotalOrders = table.Column<int>(type: "integer", nullable: false),
                    CompletedOrders = table.Column<int>(type: "integer", nullable: false),
                    PlanningId = table.Column<Guid>(type: "uuid", nullable: false),
                    YardMetricsPerStageId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_YardMetricsAppointmentsPerStage", x => x.Id);
                    table.ForeignKey(
                        name: "FK_YardMetricsAppointmentsPerStage_Plannings_PlanningId",
                        column: x => x.PlanningId,
                        principalTable: "Plannings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_YardMetricsAppointmentsPerStage_Stages_StageId",
                        column: x => x.StageId,
                        principalTable: "Stages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_YardMetricsAppointmentsPerStage_YardMetricsPerStage_YardMet~",
                        column: x => x.YardMetricsPerStageId,
                        principalTable: "YardMetricsPerStage",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_YardMetricsAppointmentsPerDock_DockId",
                table: "YardMetricsAppointmentsPerDock",
                column: "DockId");

            migrationBuilder.CreateIndex(
                name: "IX_YardMetricsAppointmentsPerDock_PlanningId",
                table: "YardMetricsAppointmentsPerDock",
                column: "PlanningId");

            migrationBuilder.CreateIndex(
                name: "IX_YardMetricsAppointmentsPerDock_YardMetricsPerDockId",
                table: "YardMetricsAppointmentsPerDock",
                column: "YardMetricsPerDockId");

            migrationBuilder.CreateIndex(
                name: "IX_YardMetricsAppointmentsPerStage_PlanningId",
                table: "YardMetricsAppointmentsPerStage",
                column: "PlanningId");

            migrationBuilder.CreateIndex(
                name: "IX_YardMetricsAppointmentsPerStage_StageId",
                table: "YardMetricsAppointmentsPerStage",
                column: "StageId");

            migrationBuilder.CreateIndex(
                name: "IX_YardMetricsAppointmentsPerStage_YardMetricsPerStageId",
                table: "YardMetricsAppointmentsPerStage",
                column: "YardMetricsPerStageId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "YardMetricsAppointmentsPerDock");

            migrationBuilder.DropTable(
                name: "YardMetricsAppointmentsPerStage");

            migrationBuilder.CreateTable(
                name: "YardMetricsAppointments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    DockId = table.Column<Guid>(type: "uuid", nullable: false),
                    PlanningId = table.Column<Guid>(type: "uuid", nullable: false),
                    YardMetricsPerDockId = table.Column<Guid>(type: "uuid", nullable: false),
                    YardMetricsPerStageId = table.Column<Guid>(type: "uuid", nullable: true),
                    AppointmentCode = table.Column<string>(type: "text", nullable: false),
                    AppointmentDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CompletedOrders = table.Column<int>(type: "integer", nullable: false),
                    Customer = table.Column<string>(type: "text", nullable: false),
                    EndDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsOnTime = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    License = table.Column<string>(type: "text", nullable: false),
                    ProcessType = table.Column<string>(type: "text", nullable: false),
                    Progress = table.Column<double>(type: "double precision", nullable: false),
                    StartDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    TotalOrders = table.Column<int>(type: "integer", nullable: false),
                    Usage = table.Column<double>(type: "double precision", nullable: false, defaultValue: 0.0),
                    VehicleType = table.Column<string>(type: "text", nullable: false),
                    YardCode = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_YardMetricsAppointments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_YardMetricsAppointments_Docks_DockId",
                        column: x => x.DockId,
                        principalTable: "Docks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_YardMetricsAppointments_Plannings_PlanningId",
                        column: x => x.PlanningId,
                        principalTable: "Plannings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_YardMetricsAppointments_YardMetricsPerDock_YardMetricsPerDo~",
                        column: x => x.YardMetricsPerDockId,
                        principalTable: "YardMetricsPerDock",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_YardMetricsAppointments_YardMetricsPerStage_YardMetricsPerS~",
                        column: x => x.YardMetricsPerStageId,
                        principalTable: "YardMetricsPerStage",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_YardMetricsAppointments_DockId",
                table: "YardMetricsAppointments",
                column: "DockId");

            migrationBuilder.CreateIndex(
                name: "IX_YardMetricsAppointments_PlanningId",
                table: "YardMetricsAppointments",
                column: "PlanningId");

            migrationBuilder.CreateIndex(
                name: "IX_YardMetricsAppointments_YardMetricsPerDockId",
                table: "YardMetricsAppointments",
                column: "YardMetricsPerDockId");

            migrationBuilder.CreateIndex(
                name: "IX_YardMetricsAppointments_YardMetricsPerStageId",
                table: "YardMetricsAppointments",
                column: "YardMetricsPerStageId");
        }
    }
}
