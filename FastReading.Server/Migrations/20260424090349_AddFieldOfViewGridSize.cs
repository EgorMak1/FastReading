using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FastReading.Server.Migrations
{
    /// <inheritdoc />
    public partial class AddFieldOfViewGridSize : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "GridSize",
                table: "FieldOfViewResults",
                type: "integer",
                nullable: false,
                defaultValue: 5);

            migrationBuilder.Sql("""
                UPDATE "FieldOfViewResults"
                SET "GridSize" = 5
                WHERE "GridSize" = 0;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "GridSize",
                table: "FieldOfViewResults");
        }
    }
}
