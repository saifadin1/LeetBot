using Discord;
using Discord.WebSocket;
using LeetBot.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LeetBot.Commands
{
    internal class LeaveFromChallangCommand : ISlashCommand
    {
        public bool isApiCommand { get; set; } = false;

        private readonly IChallengeRepo _challengeRepo;

        public LeaveFromChallangCommand(IChallengeRepo challengeRepo)
        {
            _challengeRepo = challengeRepo;
        }

        public SlashCommandBuilder BuildCommand()
        {
            return new SlashCommandBuilder()
                .WithName("leave")
                .WithDescription("use this command in case challange message has been deleted");
        }

        public async Task ExecuteAsync(SocketSlashCommand command, ISocketMessageChannel channel)
        {
            var challange = await _challengeRepo.GetChallengeByUserId($"{command.User.Id}-{command.GuildId}");
            if (challange == null)
            {
                await command.RespondAsync("You are not in a challenge.", ephemeral: true);
            } else
            {
                await _challengeRepo.RemoveChallenger($"{command.User.Id}-{command.GuildId}");
                await _challengeRepo.SaveChangesAsync();
                
                await command.RespondAsync("You have left the challenge.", ephemeral: true);
            }
        }
    }
}
