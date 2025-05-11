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
        Task<List<User>> GetUsersByGuildIdAsync(ulong? guildId);
        Task<int> SaveChangesAsync();

    }
}
