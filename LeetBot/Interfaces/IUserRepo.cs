using Discord;
using Discord.WebSocket;
using LeetBot.Models;
using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LeetBot.Interfaces
{
    public interface IUserRepo
    {
        Task<User> CreateUserAsync(IDiscordInteraction interaction);
        Task<bool> IsUserExistAsync(IDiscordInteraction interaction);
        Task<bool> IsUserFreeAsync(IDiscordInteraction interaction);
        Task LockUserAsync(IDiscordInteraction interaction);
        Task UnlockUserAsync(IDiscordInteraction interaction);
        Task UnlockUserAsync(string userId);
        Task<List<User>> GetUsersByGuildIdAsync(ulong? guildId);
        Task<User?> GetUserByIdAsync(string id);
        Task UpdateUserLeetCodeUsernameAsync(IDiscordInteraction interaction, string newLeetCodeUsername);
        Task UpdateUserCodeforcesAsync(IDiscordInteraction interaction, string handle, int rating, string rank);
        Task<int> SaveChangesAsync();

    }
}
