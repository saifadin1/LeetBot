﻿// <auto-generated />
using System;
using LeetBot.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace LeetBot.Migrations
{
    [DbContext(typeof(AppDbContext))]
    [Migration("20250515051124_IsFreeDefaultValue")]
    partial class IsFreeDefaultValue
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "9.0.4")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("LeetBot.Models.Challenge", b =>
                {
                    b.Property<decimal>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("numeric(20,0)");

                    b.Property<string>("ChallengerId")
                        .HasColumnType("text");

                    b.Property<string>("Difficulty")
                        .HasColumnType("text");

                    b.Property<decimal?>("GuildId")
                        .HasColumnType("numeric(20,0)");

                    b.Property<string>("OpponentId")
                        .HasColumnType("text");

                    b.Property<string>("ProblemLink")
                        .HasColumnType("text");

                    b.Property<DateTime>("StartedAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("TitleSlug")
                        .HasColumnType("text");

                    b.Property<string>("Topic")
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.HasIndex("ChallengerId")
                        .IsUnique();

                    b.HasIndex("OpponentId")
                        .IsUnique();

                    b.ToTable("Challenges");
                });

            modelBuilder.Entity("LeetBot.Models.Team", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<long>("Id"));

                    b.Property<long>("ChallengeId")
                        .HasColumnType("bigint");

                    b.Property<int>("Score")
                        .HasColumnType("integer");

                    b.HasKey("Id");

                    b.HasIndex("ChallengeId");

                    b.ToTable("Teams");
                });

            modelBuilder.Entity("LeetBot.Models.TeamChallenge", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<long>("Id"));

                    b.Property<string>("EasyProblemTitleSlug")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<DateTime>("EndedAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<decimal?>("GuildId")
                        .HasColumnType("numeric(20,0)");

                    b.Property<string>("HardProblemTitleSlug")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("MediumProblemTitleSlug")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<int>("Problem1SolvedByTeam")
                        .HasColumnType("integer");

                    b.Property<int>("Problem2SolvedByTeam")
                        .HasColumnType("integer");

                    b.Property<int>("Problem3SolvedByTeam")
                        .HasColumnType("integer");

                    b.Property<DateTime>("StartedAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<int>("Team1CurrentScore")
                        .HasColumnType("integer");

                    b.Property<int>("Team1MaxPossibleScore")
                        .HasColumnType("integer");

                    b.Property<int>("Team2CurrentScore")
                        .HasColumnType("integer");

                    b.Property<int>("Team2MaxPossibleScore")
                        .HasColumnType("integer");

                    b.HasKey("Id");

                    b.ToTable("TeamChallenges");
                });

            modelBuilder.Entity("LeetBot.Models.User", b =>
                {
                    b.Property<string>("Id")
                        .HasColumnType("text");

                    b.Property<int>("EasyWon")
                        .HasColumnType("integer");

                    b.Property<int>("GamePlayed")
                        .HasColumnType("integer");

                    b.Property<int>("GameWon")
                        .HasColumnType("integer");

                    b.Property<long?>("GuildId")
                        .HasColumnType("bigint");

                    b.Property<int>("HardWon")
                        .HasColumnType("integer");

                    b.Property<bool>("IsFree")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("boolean")
                        .HasDefaultValue(true);

                    b.Property<string>("LeetCodeUsername")
                        .HasColumnType("text");

                    b.Property<int>("MediumWon")
                        .HasColumnType("integer");

                    b.Property<string>("Mention")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<long?>("TeamId")
                        .HasColumnType("bigint");

                    b.Property<string>("Username")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<DateTime>("VerifiedAt")
                        .HasColumnType("timestamp with time zone");

                    b.HasKey("Id");

                    b.HasIndex("TeamId");

                    b.ToTable("Users");
                });

            modelBuilder.Entity("LeetBot.Models.Challenge", b =>
                {
                    b.HasOne("LeetBot.Models.User", "Challenger")
                        .WithOne()
                        .HasForeignKey("LeetBot.Models.Challenge", "ChallengerId")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("LeetBot.Models.User", "Opponent")
                        .WithOne()
                        .HasForeignKey("LeetBot.Models.Challenge", "OpponentId")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.Navigation("Challenger");

                    b.Navigation("Opponent");
                });

            modelBuilder.Entity("LeetBot.Models.Team", b =>
                {
                    b.HasOne("LeetBot.Models.TeamChallenge", "TeamChallenge")
                        .WithMany("Teams")
                        .HasForeignKey("ChallengeId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("TeamChallenge");
                });

            modelBuilder.Entity("LeetBot.Models.User", b =>
                {
                    b.HasOne("LeetBot.Models.Team", "Team")
                        .WithMany("Users")
                        .HasForeignKey("TeamId")
                        .OnDelete(DeleteBehavior.SetNull);

                    b.Navigation("Team");
                });

            modelBuilder.Entity("LeetBot.Models.Team", b =>
                {
                    b.Navigation("Users");
                });

            modelBuilder.Entity("LeetBot.Models.TeamChallenge", b =>
                {
                    b.Navigation("Teams");
                });
#pragma warning restore 612, 618
        }
    }
}
