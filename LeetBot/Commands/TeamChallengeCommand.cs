using Discord;
using Discord.WebSocket;
using LeetBot.Interfaces;
using LeetBot.Repositories;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Net.WebRequestMethods;

namespace LeetBot.Commands
{
    public class TeamChallengeCommand : ISlashCommand
    {
        public bool isApiCommand { get; set; } = false;

        private readonly ILogger<TeamChallengeCommand> _logger;
        private readonly IUserRepo _userRepo;

        public TeamChallengeCommand(ILogger<TeamChallengeCommand> logger, IUserRepo userRepo)
        {
            _logger = logger;
            _userRepo = userRepo;
        }

        public SlashCommandBuilder BuildCommand()
        {
            return new SlashCommandBuilder()
                .WithName("team-challenge")
                .WithDescription("Create a team challenge");
        }

        public async Task ExecuteAsync(SocketSlashCommand command, ISocketMessageChannel channel)
        {
            var userId = command.User.Id;

            var existingUser = await _userRepo.IsUserExistAsync(command);
            if (existingUser == false)
            {
                await command.RespondAsync("You need to verify yourself first using the /identify command.");
                return;
            }

            if (!await _userRepo.IsUserFreeAsync(command))
            {
                await command.RespondAsync("You are already in a challenge.", ephemeral: true);
                return;
            }
            await _userRepo.LockUserAsync(command);


            var loadingEmoji = ":Loading:";
            var embed = new EmbedBuilder()
                .WithTitle("Waiting for players...")
                .WithDescription($"Current Game: 2v2\\n\\n**Team 1:**\\n- {command.User.Mention}\\n- {loadingEmoji}\\n\\n**Team 2:**\\n- {loadingEmoji}\\n- {loadingEmoji}\"")
                .WithColor(Color.Blue);

            var components = new ComponentBuilder()
                .WithButton("Join Team 1", "joinTeam1Btn")
                .WithButton("Join Team 2", "joinTeam2Btn")
                .WithButton("leave", "leaveTeamBtn")
                .WithButton("start", "startTeamBtn");

            try
            {
                await command.DeferAsync();

                var myChannel = (SocketTextChannel)channel;
                var thread = await myChannel.CreateThreadAsync(
                    name: $"Team Challenge by {command.User.Username}",
                    autoArchiveDuration: ThreadArchiveDuration.OneHour
                );

                var message = await thread.SendMessageAsync(
                    embed: embed.Build(),
                    components: components.Build()
                );

                // Respond to the user
                await command.FollowupAsync("Team challenge created ✅", ephemeral: true);
                await Task.Delay(1500);
                await command.DeleteOriginalResponseAsync();

                // create new TeamChallenge and new two teams

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error responding to command");
                await command.FollowupAsync("An error occurred while sending the challenge.");
                return;
            }


        }


    }
}
