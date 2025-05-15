using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LeetBot.Migrations
{
    /// <inheritdoc />
    public partial class teamStats : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsFree",
                table: "Users",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AlterColumn<decimal>(
                name: "GuildId",
                table: "TeamChallenges",
                type: "numeric(20,0)",
                nullable: true,
                oldClrType: typeof(long),
                oldType: "bigint",
                oldNullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Problem1SolvedByTeam",
                table: "TeamChallenges",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Problem2SolvedByTeam",
                table: "TeamChallenges",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Problem3SolvedByTeam",
                table: "TeamChallenges",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Team1CurrentScore",
                table: "TeamChallenges",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Team1MaxPossibleScore",
                table: "TeamChallenges",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Team2CurrentScore",
                table: "TeamChallenges",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Team2MaxPossibleScore",
                table: "TeamChallenges",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsFree",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "Problem1SolvedByTeam",
                table: "TeamChallenges");

            migrationBuilder.DropColumn(
                name: "Problem2SolvedByTeam",
                table: "TeamChallenges");

            migrationBuilder.DropColumn(
                name: "Problem3SolvedByTeam",
                table: "TeamChallenges");

            migrationBuilder.DropColumn(
                name: "Team1CurrentScore",
                table: "TeamChallenges");

            migrationBuilder.DropColumn(
                name: "Team1MaxPossibleScore",
                table: "TeamChallenges");

            migrationBuilder.DropColumn(
                name: "Team2CurrentScore",
                table: "TeamChallenges");

            migrationBuilder.DropColumn(
                name: "Team2MaxPossibleScore",
                table: "TeamChallenges");

            migrationBuilder.AlterColumn<long>(
                name: "GuildId",
                table: "TeamChallenges",
                type: "bigint",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "numeric(20,0)",
                oldNullable: true);
        }
    }
}
