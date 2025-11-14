using Discord.Rest;
using Discord;
using LeetBot.Models;

namespace LeetBot.Interfaces
{
    public interface ITeamChallengeRepo
    {
        Task<TeamChallenge> CreateTeamChallengeAsync(IDiscordInteraction interaction, RestUserMessage message);
        Task<TeamChallenge?> GetTeamChallengeByIdAsync(ulong id);
        Task<List<TeamChallenge>> GetTeamChallengesByGuildIdAsync(ulong? guildId);
        Task<List<Team>> GetTeamsByTeamChallengeIdAsync(ulong teamChallengeId);
        Task<List<TeamChallenge>> GetExpiredChallengesAsync();

        Task AddTeamToChallengeAsync(ulong teamChallengeId, Team team);
        Task RemoveTeamFromChallengeAsync(ulong teamChallengeId, Team team);
        Task DeleteTeamChallengeAsync(ulong id);
        Task<int> SaveChangesAsync();
    }
}
