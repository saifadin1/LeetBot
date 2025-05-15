using Discord.Rest;
using Discord;
using LeetBot.Data;
using LeetBot.Interfaces;
using LeetBot.Models;
using Microsoft.EntityFrameworkCore;

namespace LeetBot.Repositories
{
    public class TeamChallengeRepo : ITeamChallengeRepo
    {
        private readonly AppDbContext _dbContext;

        public TeamChallengeRepo(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<TeamChallenge> CreateTeamChallengeAsync(IDiscordInteraction interaction, RestUserMessage message)
        {
            var teamChallenge = new TeamChallenge
            {
                Id = (long)message.Id,
                GuildId = interaction.GuildId,
                StartedAt = DateTime.UtcNow
            };

            await _dbContext.TeamChallenges.AddAsync(teamChallenge);
            await _dbContext.SaveChangesAsync();
            return teamChallenge;
        }

        public async Task<TeamChallenge?> GetTeamChallengeByIdAsync(long id)
        {
            return await _dbContext.TeamChallenges
                .Include(tc => tc.Teams)
                .ThenInclude(t => t.Users)
                .FirstOrDefaultAsync(tc => tc.Id == id);
        }

        public async Task<List<TeamChallenge>> GetTeamChallengesByGuildIdAsync(ulong? guildId)
        {
            return await _dbContext.TeamChallenges
                .Where(tc => tc.GuildId == guildId)
                .Include(tc => tc.Teams)
                .ToListAsync();
        }

        public async Task AddTeamToChallengeAsync(long teamChallengeId, Team team)
        {
            var teamChallenge = await _dbContext.TeamChallenges
                .Include(tc => tc.Teams)
                .FirstOrDefaultAsync(tc => tc.Id == teamChallengeId);

            if (teamChallenge == null)
                throw new Exception("TeamChallenge not found.");

            teamChallenge.Teams.Add(team);
            await _dbContext.SaveChangesAsync();
        }

        public async Task RemoveTeamFromChallengeAsync(long teamChallengeId, Team team)
        {
            var teamChallenge = await _dbContext.TeamChallenges
                .Include(tc => tc.Teams)
                .FirstOrDefaultAsync(tc => tc.Id == teamChallengeId);

            if (teamChallenge == null)
                throw new Exception("TeamChallenge not found.");

            teamChallenge.Teams.Remove(team);
            await _dbContext.SaveChangesAsync();
        }

        public async Task DeleteTeamChallengeAsync(long id)
        {
            var teamChallenge = await _dbContext.TeamChallenges.FindAsync(id);

            if (teamChallenge == null)
                throw new Exception("TeamChallenge not found.");

            _dbContext.TeamChallenges.Remove(teamChallenge);
            await _dbContext.SaveChangesAsync();
        }

        public async Task<int> SaveChangesAsync()
        {
            return await _dbContext.SaveChangesAsync();
        }

        public async Task<List<Team>> GetTeamsByTeamChallengeIdAsync(long teamChallengeId)
        {
            var teamChallenge = await _dbContext.TeamChallenges
                .Include(tc => tc.Teams)
                .ThenInclude(t => t.Users)
                .FirstOrDefaultAsync(tc => tc.Id == teamChallengeId);

            if (teamChallenge == null)
                throw new KeyNotFoundException($"TeamChallenge with ID {teamChallengeId} not found.");

            return teamChallenge.Teams.ToList();
        }
    }
}
