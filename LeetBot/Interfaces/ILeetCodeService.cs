using Discord;
using Discord.Rest;
using Discord.WebSocket;
using LeetBot.DTOs;
using LeetBot.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LeetBot.Interfaces
{
    internal interface ILeetCodeService
    {
        Task<string> GetUserRealNameAsync(string username);
        Task<string> GetRandomProblemAsync(string difficulty);
        Task<UserLastSubmissionDTO> GetUserSubmissionsAsync(string username);
        Task<List<(int, string)>> GetUserProblemSolved(string username);
        Task GetUsersSubmissions(SocketMessageComponent component, Challenge challenge, SocketThreadChannel threadChannel);
        //Task MonitorChallengeAsync(SocketMessageComponent component, Challenge challenge, SocketThreadChannel threadChannel, CancellationToken cancellationToken);
    }
}
