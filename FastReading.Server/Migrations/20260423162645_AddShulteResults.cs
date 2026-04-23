using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FastReading.Server.Migrations
{
    /// <inheritdoc />
    public partial class AddShulteResults : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ShulteResults",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    GridSize = table.Column<int>(type: "integer", nullable: false),
                    NumbersCount = table.Column<int>(type: "integer", nullable: false),
                    LevelBefore = table.Column<int>(type: "integer", nullable: false),
                    LevelAfter = table.Column<int>(type: "integer", nullable: false),
                    DurationSeconds = table.Column<int>(type: "integer", nullable: false),
                    ErrorsCount = table.Column<int>(type: "integer", nullable: false),
                    Score = table.Column<double>(type: "double precision", nullable: false),
                    CompletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ShulteResults", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ShulteResults_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ShulteResults_UserId",
                table: "ShulteResults",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ShulteResults");
        }
    }
}
