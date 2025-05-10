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
    internal interface IUserRepo
    {
        Task<User> CreateUserAsync(IDiscordInteraction interaction);
        Task<bool> IsUserExist(IDiscordInteraction interaction);
        Task<List<User>> GetUsersByGuildId(ulong? guildId);
        Task<int> SaveChangesAsync();

    }
}
