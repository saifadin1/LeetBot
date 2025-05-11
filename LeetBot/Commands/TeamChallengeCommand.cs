using Discord;
using Discord.WebSocket;
using LeetBot.Interfaces;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LeetBot.Commands
{
    public class TeamChallengeCommand : ISlashCommand
    {
        public bool isApiCommand { get; set; } = false;

        private readonly ILogger<TeamChallengeCommand> _logger;

        public TeamChallengeCommand(ILogger<TeamChallengeCommand> logger)
        {
            _logger = logger;
        }

        public SlashCommandBuilder BuildCommand()
        {
            return new SlashCommandBuilder()
                .WithName("teamchallenge")
                .WithDescription("Create a team challenge");
        }

        public Task ExecuteAsync(SocketSlashCommand command, ISocketMessageChannel channel)
        {
            throw new NotImplementedException();
        }
    }
}
