using Discord;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LeetBot.Interfaces
{
    public interface ISlashCommand
    {
        bool isApiCommand { get; set; }
        SlashCommandBuilder BuildCommand();
        Task ExecuteAsync(SocketSlashCommand command, ISocketMessageChannel channel);
    }
}
