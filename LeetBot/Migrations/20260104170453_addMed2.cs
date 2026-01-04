using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LeetBot.Migrations
{
    /// <inheritdoc />
    public partial class addMed2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "MediumProblemTitleSlug",
                table: "TeamChallenges",
                newName: "MediumProblem2TitleSlug");

            migrationBuilder.AddColumn<string>(
                name: "MediumProblem1TitleSlug",
                table: "TeamChallenges",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Problem4SolvedByTeam",
                table: "TeamChallenges",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MediumProblem1TitleSlug",
                table: "TeamChallenges");

            migrationBuilder.DropColumn(
                name: "Problem4SolvedByTeam",
                table: "TeamChallenges");

            migrationBuilder.RenameColumn(
                name: "MediumProblem2TitleSlug",
                table: "TeamChallenges",
                newName: "MediumProblemTitleSlug");
        }
    }
}
