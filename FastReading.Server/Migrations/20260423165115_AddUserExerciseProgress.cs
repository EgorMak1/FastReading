using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FastReading.Server.Migrations
{
    /// <inheritdoc />
    public partial class AddUserExerciseProgress : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "UserExerciseProgresses",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    ExerciseType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    CurrentLevel = table.Column<int>(type: "integer", nullable: false),
                    LastScore = table.Column<double>(type: "double precision", nullable: false),
                    AverageScore = table.Column<double>(type: "double precision", nullable: false),
                    BestScore = table.Column<double>(type: "double precision", nullable: false),
                    SessionsCount = table.Column<int>(type: "integer", nullable: false),
                    SuccessStreak = table.Column<int>(type: "integer", nullable: false),
                    FailStreak = table.Column<int>(type: "integer", nullable: false),
                    LastPlayedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserExerciseProgresses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserExerciseProgresses_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_UserExerciseProgresses_UserId_ExerciseType",
                table: "UserExerciseProgresses",
                columns: new[] { "UserId", "ExerciseType" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserExerciseProgresses");
        }
    }
}
