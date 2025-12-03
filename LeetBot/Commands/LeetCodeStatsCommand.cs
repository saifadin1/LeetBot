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

            var username = command.Data.Options.First(x => x.Name == "username").Value.ToString();

            var statsTask = _leetCodeService.GetNumAccQuestionsAsync(username);
            var badgesTask = _leetCodeService.GetUserBadgesAsync(username);
            var avatarTask = _leetCodeService.GetUserAvatarAsync(username);

            await Task.WhenAll(statsTask, badgesTask, avatarTask);

            var stats = statsTask.Result;
            var badges = badgesTask.Result;
            var avatarUrl = avatarTask.Result;

            if (stats == null)
            {
                await command.FollowupAsync($"Could not retrieve stats for user '{username}'.");
                return;
            }

 
            var embed = new EmbedBuilder()
                .WithTitle($"LeetCode Profile: {stats.LeetCodeUsername}")
                .WithColor(Color.Green)
                .WithUrl($"https://leetcode.com/{stats.LeetCodeUsername}/")
                .WithThumbnailUrl(avatarUrl);

            int badgeCount = badges?.Badges?.Count ?? 0;
            string badgeFieldValue = $"**{badgeCount}**";

            if (badgeCount > 0)
            {
                var recentNames = badges.Badges
                                    .OrderByDescending(b => b.CreationDate)
                                    .Take(3)
                                    .Select(b => b.DisplayName);

                badgeFieldValue = $"**{badgeCount} Total**\n" +
                                  $"Latest: {string.Join(", ", recentNames)}";
            }

            embed.AddField("🏅 Badges", badgeFieldValue, inline: true);

            int totalSolved = stats.NumAcceptedQuestions.Sum(q => q.Count);

            var statsSb = new StringBuilder();
            statsSb.AppendLine($"**Total Solved: {totalSolved}**\n");

            foreach (var item in stats.NumAcceptedQuestions)
            {
                String emoji = item.Difficulty switch
                {
                    "EASY" => "🥱",
                    "MEDIUM" => "🤓",
                    "HARD" => "🤯",
                    _ => "⚪"
                };
                statsSb.AppendLine($"{emoji} {item.Difficulty}: **{item.Count}**");
            }

            embed.AddField("Problems Solved", statsSb.ToString(), inline: true);

            // 3. Send
            await command.FollowupAsync(embed: embed.Build());
        }
    }
}
