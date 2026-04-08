using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Mss.WorkForce.Code.DataBaseManager.Migrations
{
    /// <inheritdoc />
    public partial class BufferProperties : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "NumCrossAisles",
                table: "Buffers",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "NumShelves",
                table: "Buffers",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Type",
                table: "Buffers",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "NumCrossAisles",
                table: "Buffers");

            migrationBuilder.DropColumn(
                name: "NumShelves",
                table: "Buffers");

            migrationBuilder.DropColumn(
                name: "Type",
                table: "Buffers");
        }
    }
}
