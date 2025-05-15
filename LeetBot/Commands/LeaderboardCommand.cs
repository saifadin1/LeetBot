using Discord;
using Discord.WebSocket;
using LeetBot.Interfaces;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LeetBot.Commands
{
    class LeaderboardUserDTO
    {
        public string userMention { get; set; }
        public int easySolved { get; set; }
        public int mediumSolved { get; set; }
        public int hardSolved { get; set; }
        public int totalSolved { get; set; }
    }
    internal class LeaderboardCommand : ISlashCommand
    {
        private readonly IUserRepo _userRepo;
        private readonly ILeetCodeService _leetCodeService;

        public LeaderboardCommand(IUserRepo userRepo, ILeetCodeService leetCodeService)
        {
            _leetCodeService = leetCodeService;
            _userRepo = userRepo;
        }
        public bool isApiCommand { get; set; } = true;

        public SlashCommandBuilder BuildCommand()
        {
            return new SlashCommandBuilder()
                .WithName("leaderboard")
                .WithDescription("Get the leaderboard for LeetCode");
                //.AddOption(new SlashCommandOptionBuilder()
                //    .WithName("type")
                //    .WithDescription("Select type: challenges or problems")
                //    .WithType(ApplicationCommandOptionType.String)
                //    .WithRequired(true)
                //    .AddChoice("Challenges", "challenges")
                //    .AddChoice("Problems", "problems")
                //)

        }

        public async Task ExecuteAsync(SocketSlashCommand command, ISocketMessageChannel channel)
        {
            var usersList = await _userRepo.GetUsersByGuildIdAsync(command.GuildId);
            if (usersList == null || usersList.Count == 0)
            {
                await command.RespondAsync("No users found.");
                return;
            }
            //if (command.Data.Options.First().Value.ToString() == "problems")
            //{
            //    List<LeaderboardUserDTO> leaderboard = new List<LeaderboardUserDTO>();
            //    foreach (var user in usersList)
            //    {
            //        var problemSolved = await _leetCodeService.GetUserProblemSolved(user.Username);
            //        leaderboard.Add(new LeaderboardUserDTO
            //        {
            //            userMention = user.Mention,
            //            easySolved = problemSolved.Where(p => p.Item2.ToLower() == "easy").FirstOrDefault().Item1,
            //            mediumSolved = problemSolved.Where(p => p.Item2.ToLower() == "medium").FirstOrDefault().Item1,
            //            hardSolved = problemSolved.Where(p => p.Item2.ToLower() == "hard").FirstOrDefault().Item1
            //        });

            //        leaderboard.Last().totalSolved = leaderboard.Last().easySolved + leaderboard.Last().mediumSolved + leaderboard.Last().hardSolved;



            //        await Task.Delay(500);
            //    }

            //    leaderboard = leaderboard
            //        .OrderByDescending(x => x.totalSolved)
            //        .ThenByDescending(x => x.hardSolved)
            //        .ThenByDescending(x => x.mediumSolved)
            //        .ThenByDescending(x => x.easySolved)
            //        .ToList();

            //    var embed = new EmbedBuilder()
            //        .WithTitle("🏆 LeetCode Server Leaderboard")
            //        .WithDescription("Challenge your friends and climb to the top!\n\n")
            //        .WithColor(Color.DarkPurple)
            //        .WithCurrentTimestamp();

            //    int rank = 1;
            //    foreach (var user in leaderboard)
            //    {
            //        string medal = rank switch
            //        {
            //            1 => "🥇",
            //            2 => "🥈",
            //            3 => "🥉",
            //            _ => $"#{rank}"
            //        };

            //        embed.Description +=
            //            $"{medal} {user.userMention}\n" +
            //            $"🟢 **Easy**: {user.easySolved} 🟠 **Medium**: {user.mediumSolved} 🔴 **Hard**: {user.hardSolved} 🧮 **Total**: {user.totalSolved}\n\n";

            //        rank++;
            //    }

            //    await command.RespondAsync(embed: embed.Build());
            //}
            //else
            {
                 usersList = usersList
                    .OrderByDescending(x => x.DifficultyScore)
                    .ThenByDescending(x => x.WinPercentage)
                    .ThenByDescending(u => u.GameWon)
                    .ToList();

                var embed = new EmbedBuilder()
                    .WithTitle("🏆 LeetCode Leaderboard")
                    .WithDescription("Compete by solving LeetCode problems!\nRanks are based on difficulty-weighted score and win rate.")
                    .WithColor(Color.Gold)
                    .WithFooter("Scoring: Easy=1pt | Medium=2pts | Hard=3pts | Sorted by score & win rate");

                var currentUser = usersList.FirstOrDefault(u => u.Mention == command.User.Mention);
                int currentUserRank = currentUser is not null ? usersList.IndexOf(currentUser) + 1 : -1;

                embed.WithTitle("🏆 Leaderboard")
                     .WithDescription("Top 5 Players\n");

                for (int i = 0; i < usersList.Count && i < 5; i++)
                {
                    var user = usersList[i];
                    string isYou = currentUser is not null && user.Mention == currentUser.Mention ? " 🔹(You)" : "";
                    string medal = i switch
                    {
                        0 => "🥇",
                        1 => "🥈",
                        2 => "🥉",
                        _ => $"#{i + 1}"
                    };

                    embed.AddField(
                        $"{medal} {user.Username}{isYou}",
                        $"Score: {user.DifficultyScore} | Win %: {user.WinPercentage}% | Wins: {user.GameWon}/{user.GamePlayed}",
                        inline: false
                    );
                }

                if (currentUser is not null && currentUserRank > 5)
                {
                    embed.AddField(
                        $"Your Rank: #{currentUserRank}",
                        $"👤 {currentUser.Username} | Score: {currentUser.DifficultyScore} | Win %: {currentUser.WinPercentage}% | Wins: {currentUser.GameWon}/{currentUser.GamePlayed}",
                        inline: false
                    );
                }

                await command.RespondAsync(embed: embed.Build());
                   
            }
        }
    }
}
