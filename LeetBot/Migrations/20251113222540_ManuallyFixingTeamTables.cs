using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace LeetBot.Migrations
{
    /// <inheritdoc />
    public partial class ManuallyFixingTeamTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // === PART 1: DROP ALL DEPENDENCIES & TABLES ===
            // We must drop keys in order from "child" to "parent"

            // 1. Drop the Foreign Key from "Users" -> "Teams"
            // (EF Core default name: FK_Users_Teams_TeamId)
            migrationBuilder.DropForeignKey(
                name: "FK_Users_Teams_TeamId",
                table: "Users");

            // 2. Drop the index for that key
            migrationBuilder.DropIndex(
                name: "IX_Users_TeamId",
                table: "Users");

            // 3. Drop the Foreign Key from "Teams" -> "TeamChallenges"
            // (This was the source of your original error)
            migrationBuilder.DropForeignKey(
                name: "FK_Teams_TeamChallenges_ChallengeId",
                table: "Teams");

            // 4. Drop the "Teams" table
            migrationBuilder.DropTable(
                name: "Teams");

            // 5. Drop the "TeamChallenges" table
            migrationBuilder.DropTable(
                name: "TeamChallenges");


            // === PART 2: RE-CREATE TABLES WITH CORRECT 'ulong' TYPES ===

            // 1. Create "TeamChallenges" table first (the "parent")
            migrationBuilder.CreateTable(
                name: "TeamChallenges",
                columns: table => new
                {
                    // This is 'ulong' -> numeric(20,0)
                    Id = table.Column<decimal>(type: "numeric(20,0)", nullable: false),

                    // --- All columns from your TeamChallenge.cs model ---
                    GuildId = table.Column<decimal>(type: "numeric(20,0)", nullable: true),
                    ChannelId = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    StartedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EndedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EasyProblemTitleSlug = table.Column<string>(type: "text", nullable: true),
                    MediumProblemTitleSlug = table.Column<string>(type: "text", nullable: true),
                    HardProblemTitleSlug = table.Column<string>(type: "text", nullable: true),
                    Team1CurrentScore = table.Column<int>(type: "integer", nullable: false),
                    Team2CurrentScore = table.Column<int>(type: "integer", nullable: false),
                    Team1MaxPossibleScore = table.Column<int>(type: "integer", nullable: false),
                    Team2MaxPossibleScore = table.Column<int>(type: "integer", nullable: false),
                    Problem1SolvedByTeam = table.Column<int>(type: "integer", nullable: false),
                    Problem2SolvedByTeam = table.Column<int>(type: "integer", nullable: false),
                    Problem3SolvedByTeam = table.Column<int>(type: "integer", nullable: false),
                    // From BaseChallenge.cs
                    IsActive = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TeamChallenges", x => x.Id);
                });

            // 2. Create "Teams" table
            migrationBuilder.CreateTable(
                name: "Teams",
                columns: table => new
                {
                    // This is 'long' -> bigint, with auto-increment
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),

                    // This is 'ulong' -> numeric(20,0), the new foreign key
                    ChallengeId = table.Column<decimal>(type: "numeric(20,0)", nullable: false),

                    Score = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Teams", x => x.Id);

                    // This re-creates the foreign key correctly
                    table.ForeignKey(
                        name: "FK_Teams_TeamChallenges_ChallengeId",
                        column: x => x.ChallengeId,
                        principalTable: "TeamChallenges",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade); // You can change this if needed
                });


            migrationBuilder.Sql("UPDATE \"Users\" SET \"TeamId\" = NULL;");

            // === PART 3: RE-LINK THE 'Users' TABLE ===

            // 1. Re-create the index for "Users" -> "Teams"
            migrationBuilder.CreateIndex(
                name: "IX_Users_TeamId",
                table: "Users",
                column: "TeamId");

            // 2. Re-create the Foreign Key from "Users" -> "Teams"
            migrationBuilder.AddForeignKey(
                name: "FK_Users_Teams_TeamId",
                table: "Users",
                column: "TeamId",
                principalTable: "Teams",
                principalColumn: "Id"); // No cascade delete by default, which is safe
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // This is the reverse of what we did above

            migrationBuilder.DropForeignKey(
                name: "FK_Users_Teams_TeamId",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Users_TeamId",
                table: "Users");

            migrationBuilder.DropTable(
                name: "Teams");

            migrationBuilder.DropTable(
                name: "TeamChallenges");

            // (You would add code here to re-create the OLD (long) tables if you wanted to roll back)
        }
    }
}