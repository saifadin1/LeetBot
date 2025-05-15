using LeetBot.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LeetBot.Data
{
    public class AppDbContext: DbContext
    {
        public DbSet<User> Users => Set<User>();
        public DbSet<Challenge> Challenges => Set<Challenge>();
        public DbSet<TeamChallenge> TeamChallenges => Set<TeamChallenge>();
        public DbSet<Team> Teams => Set<Team>();


        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                IConfigurationRoot configuration = new ConfigurationBuilder()
                   .SetBasePath(Directory.GetCurrentDirectory())
                   .AddJsonFile("appsettings.json")
                   .Build();
                var connectionString = configuration.GetConnectionString("DefaultConnection");
                optionsBuilder.UseNpgsql(connectionString);
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Challenge>(entity =>
            {
                entity.HasOne(c => c.Challenger)
                      .WithOne() 
                      .HasForeignKey<Challenge>(c => c.ChallengerId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(c => c.Opponent)
                      .WithOne()
                      .HasForeignKey<Challenge>(c => c.OpponentId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<TeamChallenge>(entity =>
            {
                entity.HasMany(tc => tc.Teams)
                      .WithOne(t => t.TeamChallenge)
                      .HasForeignKey(t => t.ChallengeId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<Team>(entity =>
            {
                entity.HasMany(t => t.Users)
                      .WithOne(u => u.Team)
                      .HasForeignKey(u => u.TeamId)
                      .OnDelete(DeleteBehavior.SetNull);
            });

            modelBuilder.Entity<User>()
                .Property(u => u.IsFree)
                .HasDefaultValue(true);
        }
    }

    public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
    {
        public AppDbContext CreateDbContext(string[] args)
        {
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .Build();

            var builder = new DbContextOptionsBuilder<AppDbContext>();
            var connectionString = configuration.GetConnectionString("DefaultConnection");
            builder.UseNpgsql(connectionString);

            return new AppDbContext(builder.Options);
        }
    }
}
