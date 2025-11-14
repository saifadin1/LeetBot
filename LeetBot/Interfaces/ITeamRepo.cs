using Discord.Rest;
using Discord;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeetBot.Models;

namespace LeetBot.Interfaces
{
    public interface ITeamRepo
    {
        Task<Team> CreateTeamAsync(ulong challengeId);
        Task<Team?> GetTeamByIdAsync(long teamId);
        Task<List<Team>> GetTeamsByChallengeIdAsync(ulong challengeId);
        Task AddUserToTeamAsync(long teamId, User user);
        Task RemoveUserFromTeamAsync(long? teamId, User user);
        Task DeleteTeamAsync(long teamId);
        Task<int> SaveChangesAsync();
    }
}
