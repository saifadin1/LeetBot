using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LeetBot.Migrations
{
    /// <inheritdoc />
    public partial class cf : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CodeforcesHandle",
                table: "Users",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CodeforcesRank",
                table: "Users",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CodeforcesRating",
                table: "Users",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CodeforcesVerifiedAt",
                table: "Users",
                type: "timestamp with time zone",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CodeforcesHandle",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "CodeforcesRank",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "CodeforcesRating",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "CodeforcesVerifiedAt",
                table: "Users");
        }
    }
}
