using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Mss.WorkForce.Code.DataBaseManager.Migrations
{
    /// <inheritdoc />
    public partial class PackingRelations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Packing",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ProcessId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Packing", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Packing_Processes_ProcessId",
                        column: x => x.ProcessId,
                        principalTable: "Processes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PackingMode",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PackingType = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PackingMode", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PackingPacksMode",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PackingId = table.Column<Guid>(type: "uuid", nullable: false),
                    PackingModeId = table.Column<Guid>(type: "uuid", nullable: false),
                    NumPackages = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PackingPacksMode", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PackingPacksMode_PackingMode_PackingModeId",
                        column: x => x.PackingModeId,
                        principalTable: "PackingMode",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PackingPacksMode_Packing_PackingId",
                        column: x => x.PackingId,
                        principalTable: "Packing",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Packing_ProcessId",
                table: "Packing",
                column: "ProcessId");

            migrationBuilder.CreateIndex(
                name: "IX_PackingPacksMode_PackingId",
                table: "PackingPacksMode",
                column: "PackingId");

            migrationBuilder.CreateIndex(
                name: "IX_PackingPacksMode_PackingModeId",
                table: "PackingPacksMode",
                column: "PackingModeId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PackingPacksMode");

            migrationBuilder.DropTable(
                name: "PackingMode");

            migrationBuilder.DropTable(
                name: "Packing");
        }
    }
}
