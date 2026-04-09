using System;
using FastReading.Server.Data;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FastReading.Server.Migrations
{
    [DbContext(typeof(AppDbContext))]
    [Migration("20260409160000_AddWordErasingResults")]
    public partial class AddWordErasingResults : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "WordErasingResults",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    TextId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    TextTitle = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    SpeedBeforeWpm = table.Column<int>(type: "integer", nullable: false),
                    SpeedAfterWpm = table.Column<int>(type: "integer", nullable: false),
                    SpeedDelta = table.Column<int>(type: "integer", nullable: false),
                    CompletionType = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    CorrectAnswers = table.Column<int>(type: "integer", nullable: false),
                    TotalQuestions = table.Column<int>(type: "integer", nullable: false),
                    QuestionsSkipped = table.Column<bool>(type: "boolean", nullable: false),
                    AccuracyPercent = table.Column<double>(type: "double precision", nullable: false),
                    ErasedWords = table.Column<int>(type: "integer", nullable: false),
                    TotalWords = table.Column<int>(type: "integer", nullable: false),
                    CompletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WordErasingResults", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WordErasingResults_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_WordErasingResults_UserId",
                table: "WordErasingResults",
                column: "UserId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "WordErasingResults");
        }
    }
}
