using Discord;
using Discord.Interactions;
using Discord.Rest;
using Discord.WebSocket;
using LeetBot.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LeetBot.Interfaces;

internal interface IChallengeRepo
{
    Task CreateChallengAsync(IDiscordInteraction interaction, RestUserMessage message, string difficulty, string? topic);
    Task<Challenge> GetChallengeById(ulong id);
    Task<bool> IsUserChallenging(IDiscordInteraction interaction);
    Task<int> SaveChangesAsync();
    void RemoveChallenge(ulong id);
    Task RemoveChallenger(string userId);
    Task<Challenge> GetChallengeByUserId(string userId);
    Task<bool> isEmpty(ulong id);

}
