using Discord;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LeetBot.Interfaces
{
    public interface ITeamService
    {
        Task HandleJoinTeamAsync(SocketMessageComponent component, int teamNumber);
        Task HandleDifficultyButton(SocketMessageComponent component, SocketThreadChannel threadChannel, string difficulty);
        Task<Embed> BuildTeamChallengeResultEmbedAsync(ulong challengeId);


    }
}
