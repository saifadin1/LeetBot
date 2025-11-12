using Discord;
using Discord.WebSocket;
using LeetBot.Interfaces;
using LeetBot.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LeetBot.Commands
{
    internal class LeetCodeStatsCommand : ISlashCommand
    {
        public bool isApiCommand { get; set; } = true;

        private readonly ILeetCodeService _leetCodeService;
        public LeetCodeStatsCommand(ILeetCodeService leetCodeService)
        {
            _leetCodeService = leetCodeService;
        }

        public SlashCommandBuilder BuildCommand()
        {
            return new SlashCommandBuilder()
                .WithName("lcstats")
                .WithDescription("Get LeetCode statistics for a user")
                .AddOption(new SlashCommandOptionBuilder()
                    .WithName("username")
                    .WithDescription("LeetCode username to fetch stats for")
                    .WithType(ApplicationCommandOptionType.String)
                    .WithRequired(true)
                );
        }

        public async Task ExecuteAsync(SocketSlashCommand command, ISocketMessageChannel channel)
        {
            await command.DeferAsync();

            var leetCodeUsername = command.Data.Options
                .FirstOrDefault(x => x.Name == "username")?.Value?.ToString();

            var result = await _leetCodeService.GetNumAccQuestionsAsync(leetCodeUsername);

            if (result != null)
            {
                var embed = new EmbedBuilder()
                    .WithTitle($"LeetCode Stats for {result.LeetCodeUsername}")
                    .WithColor(Color.Green);
                var sb = new StringBuilder();
                foreach (var item in result.NumAcceptedQuestions)
                {
                    sb.AppendLine($"{item.Difficulty}: {item.Count}");
                }
                embed.AddField("Accepted Questions", sb.ToString());
                await command.FollowupAsync(embed: embed.Build());
            }
            else
            {
                await command.FollowupAsync($"Could not retrieve stats for user '{leetCodeUsername}'. Please ensure the username is correct.");
            }
        }
    }
}
