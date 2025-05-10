using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LeetBot.Migrations
{
    /// <inheritdoc />
    public partial class Init : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    Username = table.Column<string>(type: "text", nullable: false),
                    LeetCodeUsername = table.Column<string>(type: "text", nullable: true),
                    GuildId = table.Column<long>(type: "bigint", nullable: true),
                    Mention = table.Column<string>(type: "text", nullable: false),
                    VerifiedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    GamePlayed = table.Column<int>(type: "integer", nullable: false),
                    GameWon = table.Column<int>(type: "integer", nullable: false),
                    EasyWon = table.Column<int>(type: "integer", nullable: false),
                    MediumWon = table.Column<int>(type: "integer", nullable: false),
                    HardWon = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Challenges",
                columns: table => new
                {
                    Id = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    TitleSlug = table.Column<string>(type: "text", nullable: true),
                    Difficulty = table.Column<string>(type: "text", nullable: true),
                    ProblemLink = table.Column<string>(type: "text", nullable: true),
                    ChallengerId = table.Column<string>(type: "text", nullable: true),
                    OpponentId = table.Column<string>(type: "text", nullable: true),
                    GuildId = table.Column<decimal>(type: "numeric(20,0)", nullable: true),
                    StartedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Challenges", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Challenges_Users_ChallengerId",
                        column: x => x.ChallengerId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Challenges_Users_OpponentId",
                        column: x => x.OpponentId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Challenges_ChallengerId",
                table: "Challenges",
                column: "ChallengerId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Challenges_OpponentId",
                table: "Challenges",
                column: "OpponentId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Challenges");

            migrationBuilder.DropTable(
                name: "Users");
        }
    }
}
