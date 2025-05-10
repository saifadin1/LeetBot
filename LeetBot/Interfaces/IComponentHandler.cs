using Discord;
using Discord.Rest;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LeetBot.Interfaces
{
    public interface IComponentHandler
    {
        string CustomId { get; }
        Task ExecuteAsync(SocketMessageComponent component, SocketThreadChannel threadChannel);
    }
}
