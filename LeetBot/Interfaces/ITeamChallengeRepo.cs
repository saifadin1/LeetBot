using Discord.Rest;
using Discord;
using LeetBot.Models;

namespace LeetBot.Interfaces
{
    public interface ITeamChallengeRepo
    {
        Task<TeamChallenge> CreateTeamChallengeAsync(IDiscordInteraction interaction, RestUserMessage message);
        Task<TeamChallenge?> GetTeamChallengeByIdAsync(long id);
        Task<List<TeamChallenge>> GetTeamChallengesByGuildIdAsync(ulong? guildId);
        Task<List<Team>> GetTeamsByTeamChallengeIdAsync(long teamChallengeId);
        Task AddTeamToChallengeAsync(long teamChallengeId, Team team);
        Task RemoveTeamFromChallengeAsync(long teamChallengeId, Team team);
        Task DeleteTeamChallengeAsync(long id);
        Task<int> SaveChangesAsync();
    }
}
