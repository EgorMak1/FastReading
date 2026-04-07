using System;
using FastReading.Server.Data;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FastReading.Server.Migrations
{
    [DbContext(typeof(AppDbContext))]
    [Migration("20260407143000_AddFieldOfViewResults")]
    public partial class AddFieldOfViewResults : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "FieldOfViewResults",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    TotalRounds = table.Column<int>(type: "integer", nullable: false),
                    CorrectRounds = table.Column<int>(type: "integer", nullable: false),
                    DetectedMismatchCount = table.Column<int>(type: "integer", nullable: false),
                    MissedMismatchCount = table.Column<int>(type: "integer", nullable: false),
                    FalseAlarmCount = table.Column<int>(type: "integer", nullable: false),
                    AccuracyPercent = table.Column<double>(type: "double precision", nullable: false),
                    FinalLevel = table.Column<int>(type: "integer", nullable: false),
                    FinalIntervalMilliseconds = table.Column<int>(type: "integer", nullable: false),
                    CompletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FieldOfViewResults", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FieldOfViewResults_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_FieldOfViewResults_UserId",
                table: "FieldOfViewResults",
                column: "UserId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FieldOfViewResults");
        }
    }
}
