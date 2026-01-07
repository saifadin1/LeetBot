using LeetBot.Models;
using LeetBot.Interfaces;
using Microsoft.EntityFrameworkCore;
using LeetBot.Data;

namespace LeetBot.Repositories
{
    public class TeamRepo : ITeamRepo
    {
        private readonly AppDbContext _dbContext;

        public TeamRepo(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<Team> CreateTeamAsync(ulong challengeId)
        {
            var team = new Team
            {
                ChallengeId = challengeId
            };

            _dbContext.Teams.Add(team);
            await _dbContext.SaveChangesAsync();
            return team;
        }

        public async Task<Team?> GetTeamByIdAsync(long teamId)
        {
            return await _dbContext.Teams
                .Include(t => t.Users)
                .Include(t => t.TeamChallenge)
                .FirstOrDefaultAsync(t => t.Id == teamId);
        }

        public async Task<List<Team>> GetTeamsByChallengeIdAsync(ulong challengeId)
        {
            return await _dbContext.Teams
                .Where(t => t.ChallengeId == challengeId)
                .Include(t => t.Users)
                .ToListAsync();
        }

        public async Task AddUserToTeamAsync(long teamId, User user)
        {
            var team = await _dbContext.Teams
                .Include(t => t.Users)
                .FirstOrDefaultAsync(t => t.Id == teamId);

            if (team == null)
                throw new Exception("Team not found.");

            var existingUser = await _dbContext.Users.FindAsync(user.Id);
            if (existingUser == null)
                throw new Exception("User not found.");

            team.Users.Add(user);

            await _dbContext.SaveChangesAsync();
        }

        public async Task RemoveUserFromTeamAsync(long? teamId, User user)
        {
            var team = await _dbContext.Teams
                .Include(t => t.Users)
                .FirstOrDefaultAsync(t => t.Id == teamId);

            if (team == null)
                throw new Exception("Team not found.");

            team.Users.Remove(user);

            await _dbContext.SaveChangesAsync();
        }

        public async Task DeleteTeamAsync(long teamId)
        {
            var team = await _dbContext.Teams.FindAsync(teamId);

            if (team == null)
                throw new Exception("Team not found.");

            _dbContext.Teams.Remove(team);
            await _dbContext.SaveChangesAsync();
        }

        public async Task<int> SaveChangesAsync()
        {
            return await _dbContext.SaveChangesAsync();
        }
    }
}
