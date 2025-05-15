using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LeetBot.Migrations
{
    /// <inheritdoc />
    public partial class problemsSlugs : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "EasyProblemTitleSlug",
                table: "TeamChallenges",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "HardProblemTitleSlug",
                table: "TeamChallenges",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "MediumProblemTitleSlug",
                table: "TeamChallenges",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EasyProblemTitleSlug",
                table: "TeamChallenges");

            migrationBuilder.DropColumn(
                name: "HardProblemTitleSlug",
                table: "TeamChallenges");

            migrationBuilder.DropColumn(
                name: "MediumProblemTitleSlug",
                table: "TeamChallenges");
        }
    }
}
