using Discord;
using Discord.WebSocket;
using LeetBot.Data;
using LeetBot.Interfaces;
using LeetBot.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LeetBot.Repositories
{
    public class UserRepo : IUserRepo
    {
        private readonly AppDbContext _dbContext;
        public UserRepo(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }
        public async Task<User> CreateUserAsync(IDiscordInteraction interaction)
        {
            var user = new Models.User
            {
                Id = $"{interaction.User.Id}-{interaction.GuildId}",
                Username = interaction.User.Username,
                GuildId = (long?)interaction.GuildId,
                Mention = interaction.User.Mention
            };
            await _dbContext.Users.AddAsync(user);
            return user;
        }

        public async Task<List<User>> GetUsersByGuildIdAsync(ulong? guildId)
        {
            var users = await _dbContext.Users
                .Where(u => u.GuildId == (long)guildId)
                .ToListAsync();
            return users;
        }

        public async Task<bool> IsUserExistAsync(IDiscordInteraction interaction)
        {
            var existingUser = await _dbContext.Users.FirstOrDefaultAsync(u => u.Id == $"{interaction.User.Id}-{interaction.GuildId}");
            return existingUser != null;
        }

        public async Task<bool> IsUserFreeAsync(IDiscordInteraction interaction)
        {
            var user = await _dbContext.Users.FindAsync($"{interaction.User.Id}-{interaction.GuildId}");
            if (user == null)
            {
                return true; // User is free if not found
            }
            return user.IsFree;
        }

        public async Task LockUserAsync(IDiscordInteraction interaction)
        {
            var userId = $"{interaction.User.Id}-{interaction.GuildId}";
            var user = await _dbContext.Users.FindAsync(userId);
            if (user != null)
            {
                user.IsFree = false;
                _dbContext.Users.Update(user);
            }
        }

        public async Task<int> SaveChangesAsync()
        {
            return await _dbContext.SaveChangesAsync();
        }
    }
}
