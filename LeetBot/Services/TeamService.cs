using Discord;
using Discord.WebSocket;
using LeetBot.ComponentHandlers.TeamChallenge.Joins;
using LeetBot.DTOs;
using LeetBot.Helpers;
using LeetBot.Interfaces;
using LeetBot.Models;
using Microsoft.Extensions.Logging;
using Color = Discord.Color;

namespace LeetBot.Services
{
    public class TeamService : ITeamService
    {
        private readonly ITeamChallengeRepo _teamChallengeRepo;
        private readonly IUserRepo _userRepo;
        private readonly ITeamRepo _teamRepo;
        private readonly ILogger<JoinTeam1BtnHandler> _logger;
        private readonly ILeetCodeService _leet;

        public TeamService(ITeamChallengeRepo teamChallengeRepo,
            IUserRepo userRepo,
            ILogger<JoinTeam1BtnHandler> logger,
            ITeamRepo teamRepo,
            ILeetCodeService leet)
        {
            _teamChallengeRepo = teamChallengeRepo;
            _userRepo = userRepo;
            _logger = logger;
            _teamRepo = teamRepo;
            _leet = leet;
        }

        public async Task<Embed> BuildTeamChallengeResultEmbedAsync(ulong challengeId)
        {
            var challenge = await _teamChallengeRepo.GetTeamChallengeByIdAsync(challengeId);

            if (challenge == null)
            {
                _logger.LogWarning("Could not find TeamChallenge {ChallengeId} to build results.", challengeId);
                return new EmbedBuilder()
                    .WithTitle("Challenge Not Found")
                    .WithDescription($"Could not calculate results for challenge {challengeId} as it was not found.")
                    .WithColor(Color.Red)
                    .Build();
            }

            string title;
            string description;
            Color color;

            var team1Score = challenge.Team1CurrentScore;
            var team2Score = challenge.Team2CurrentScore;

            if (team1Score > team2Score)
            {
                title = "🏆 Team 1 Victory!";
                color = new Color(88, 101, 242); // Blurple
                description = $"Team 1 wins with a final score of **{team1Score}** to **{team2Score}**!";
            }
            else if (team2Score > team1Score)
            {
                title = "🏆 Team 2 Victory!";
                color = new Color(237, 66, 69); // Red
                description = $"Team 2 wins with a final score of **{team2Score}** to **{team1Score}**!";
            }
            else
            {
                title = "🤝 It's a Draw!";
                color = Color.LightGrey;
                description = $"Both teams tied with a score of **{team1Score}**!";
            }

            var embedBuilder = new EmbedBuilder()
                .WithTitle(title)
                .WithDescription(description)
                .WithColor(color)
                .WithCurrentTimestamp()
                .WithFooter("Challenge Complete • Thanks for playing!");

            // Calculate duration
            if (challenge.StartedAt != default && challenge.EndedAt != default)
            {
                var duration = challenge.EndedAt - challenge.StartedAt;
                embedBuilder.AddField("⏱️ Duration",
                    $"{duration.Hours}h {duration.Minutes}m {duration.Seconds}s",
                    inline: true);
            }

            embedBuilder.AddField("\u200B", "\u200B", inline: false); // Spacer

            // Add problem information with solved status
            string GetProblemStatus(int solvedBy, string slug)
            {
                if (solvedBy == 0) return $"❌ [{slug}]({TextProcessor.ProblemLink(slug)}) - *Unsolved*";
                return $"✅ [{slug}]({TextProcessor.ProblemLink(slug)}) - Team {solvedBy}";
            }

            embedBuilder.AddField("📝 Problems & Results",
                $"🟢 **Easy (100 pts):** {GetProblemStatus(challenge.Problem1SolvedByTeam, challenge.EasyProblemTitleSlug)}\n" +
                $"🟡 **Medium 1 (200 pts):** {GetProblemStatus(challenge.Problem2SolvedByTeam, challenge.MediumProblem1TitleSlug)}\n" +
                $"🟡 **Medium 2 (200 pts):** {GetProblemStatus(challenge.Problem3SolvedByTeam, challenge.MediumProblem2TitleSlug)}\n" +
                $"🔴 **Hard (400 pts):** {GetProblemStatus(challenge.Problem4SolvedByTeam, challenge.HardProblemTitleSlug)}",
                inline: false);

            embedBuilder.AddField("\u200B", "\u200B", inline: false); // Spacer

            if (challenge.Teams != null && challenge.Teams.Count >= 2)
            {
                var teams = challenge.Teams.ToList();
                var team1Users = teams[0].Users;
                var team2Users = teams[1].Users;

                var team1Members = team1Users.Any() ? string.Join("\n", team1Users.Select(u => u.Mention)) : "*No members*";
                var team2Members = team2Users.Any() ? string.Join("\n", team2Users.Select(u => u.Mention)) : "*No members*";

                embedBuilder.AddField("👥 Team 1", team1Members, inline: true);
                embedBuilder.AddField("📊 Score", $"**{challenge.Team1CurrentScore}** pts", inline: true);
                embedBuilder.AddField("\u200B", "\u200B", inline: true); // Spacer

                embedBuilder.AddField("👥 Team 2", team2Members, inline: true);
                embedBuilder.AddField("📊 Score", $"**{challenge.Team2CurrentScore}** pts", inline: true);
                embedBuilder.AddField("\u200B", "\u200B", inline: true); // Spacer
            }

            return embedBuilder.Build();
        }

        private async Task UpdateChallengeEmbed(SocketMessageComponent component, TeamChallenge challenge)
        {
            var teams = challenge.Teams.ToList();
            var team1 = teams.FirstOrDefault();
            var team2 = teams.LastOrDefault();

            var team1Members = team1?.Users.Any() == true
                ? string.Join(", ", team1.Users.Select(u => u.Mention))
                : "*No members*";
            var team2Members = team2?.Users.Any() == true
                ? string.Join(", ", team2.Users.Select(u => u.Mention))
                : "*No members*";

            var embed = new EmbedBuilder()
                .WithTitle("🎯 Team Challenge - IN PROGRESS")
                .WithDescription("First team to solve wins the points!")
                .WithColor(new Color(88, 101, 242))
                .WithCurrentTimestamp();

            embed.AddField("👥 Team 1", team1Members, inline: true);
            embed.AddField("👥 Team 2", team2Members, inline: true);
            embed.AddField("\u200B", "\u200B", inline: false);

            // Build problems list with status
            string GetProblemLine(string emoji, string name, int points, string slug, int solvedBy)
            {
                if (solvedBy == 0)
                    return $"{emoji} **{name} ({points} pts):** [{slug}]({TextProcessor.ProblemLink(slug)})";
                else
                    return $"{emoji} ~~**{name} ({points} pts)**~~: [{slug}]({TextProcessor.ProblemLink(slug)}) ✅ Team {solvedBy}";
            }

            embed.AddField("📝 Problems",
                GetProblemLine("🟢", "Easy", 100, challenge.EasyProblemTitleSlug, challenge.Problem1SolvedByTeam) + "\n" +
                GetProblemLine("🟡", "Medium 1", 200, challenge.MediumProblem1TitleSlug, challenge.Problem2SolvedByTeam) + "\n" +
                GetProblemLine("🟡", "Medium 2", 200, challenge.MediumProblem2TitleSlug, challenge.Problem3SolvedByTeam) + "\n" +
                GetProblemLine("🔴", "Hard", 400, challenge.HardProblemTitleSlug, challenge.Problem4SolvedByTeam),
                inline: false);

            embed.AddField("📊 Current Score",
                $"Team 1: **{challenge.Team1CurrentScore}** pts | Team 2: **{challenge.Team2CurrentScore}** pts",
                inline: false);

            embed.WithFooter($"Click a button after solving • Team 1 max: {challenge.Team1MaxPossibleScore} | Team 2 max: {challenge.Team2MaxPossibleScore}");

            // Update buttons - disable solved problems
            var components = new ComponentBuilder()
                .WithButton("Easy", "teamEasy", ButtonStyle.Success, new Emoji("🟢"), disabled: challenge.Problem1SolvedByTeam != 0)
                .WithButton("Medium 1", "teamMedium1", ButtonStyle.Primary, new Emoji("🟡"), disabled: challenge.Problem2SolvedByTeam != 0)
                .WithButton("Medium 2", "teamMedium2", ButtonStyle.Primary, new Emoji("🟡"), disabled: challenge.Problem3SolvedByTeam != 0)
                .WithButton("Hard", "teamHard", ButtonStyle.Danger, new Emoji("🔴"), disabled: challenge.Problem4SolvedByTeam != 0);

            await component.ModifyOriginalResponseAsync(msg =>
            {
                msg.Embed = embed.Build();
                msg.Components = components.Build();
            });
        }

        public async Task HandleDifficultyButton(SocketMessageComponent component, SocketThreadChannel threadChannel, string difficulty)
        {
            bool isVerified = await _userRepo.IsUserExistAsync(component);
            if (!isVerified)
            {
                await component.FollowupAsync("❌ You need to verify yourself first using the /identify command.", ephemeral: true);
                return;
            }

            var challenge = await _teamChallengeRepo.GetTeamChallengeByIdAsync(component.Message.Id);

            if (challenge == null)
            {
                _logger.LogError("Challenge not found.");
                await component.FollowupAsync("❌ Challenge not found.", ephemeral: true);
                return;
            }

            string problemSlug;
            int scoreToBeAdded;
            bool isAlreadySolved;

            switch (difficulty.ToLower())
            {
                case "easy":
                    problemSlug = challenge.EasyProblemTitleSlug;
                    scoreToBeAdded = 100;
                    isAlreadySolved = challenge.Problem1SolvedByTeam != 0;
                    break;
                case "medium1":
                    problemSlug = challenge.MediumProblem1TitleSlug;
                    scoreToBeAdded = 200;
                    isAlreadySolved = challenge.Problem2SolvedByTeam != 0;
                    break;
                case "medium2":
                    problemSlug = challenge.MediumProblem2TitleSlug;
                    scoreToBeAdded = 200;
                    isAlreadySolved = challenge.Problem3SolvedByTeam != 0;
                    break;
                case "hard":
                    problemSlug = challenge.HardProblemTitleSlug;
                    scoreToBeAdded = 400;
                    isAlreadySolved = challenge.Problem4SolvedByTeam != 0;
                    break;
                default:
                    await component.FollowupAsync("❌ Invalid difficulty.", ephemeral: true);
                    return;
            }

            if (isAlreadySolved)
            {
                await component.FollowupAsync("⚠️ This problem has already been solved!", ephemeral: true);
                return;
            }

            var teams = challenge.Teams;
            var allSubmissions = new List<UserLastSubmissionDTO>();

            foreach (var user in teams.First().Users)
            {
                var lastSubmission = await _leet.GetUserSubmissionsAsync(user.LeetCodeUsername);
                if (lastSubmission != null && lastSubmission.TitleSlug == problemSlug)
                {
                    allSubmissions.Add(lastSubmission);
                }
            }

            foreach (var user in teams.Last().Users)
            {
                var lastSubmission = await _leet.GetUserSubmissionsAsync(user.LeetCodeUsername);
                if (lastSubmission != null && lastSubmission.TitleSlug == problemSlug)
                {
                    allSubmissions.Add(lastSubmission);
                }
            }

            allSubmissions.Sort();

            if (allSubmissions.Count == 0)
            {
                await component.FollowupAsync("❌ No submissions found for this problem. Please click this button ONLY after you've solved the problem!", ephemeral: true);
                return;
            }

            var firstSolverSubmission = allSubmissions.FirstOrDefault();
            var firstSolver = firstSolverSubmission?.LeetCodeUsername;

            int firstSolverTeam;

            if (teams.First().Users.Any(x => x.LeetCodeUsername == firstSolver))
            {
                firstSolverTeam = 1;
                challenge.Team1CurrentScore += scoreToBeAdded;
                challenge.Team2MaxPossibleScore -= scoreToBeAdded;

                switch (difficulty.ToLower())
                {
                    case "easy": challenge.Problem1SolvedByTeam = 1; break;
                    case "medium1": challenge.Problem2SolvedByTeam = 1; break;
                    case "medium2": challenge.Problem3SolvedByTeam = 1; break;
                    case "hard": challenge.Problem4SolvedByTeam = 1; break;
                }
            }
            else
            {
                firstSolverTeam = 2;
                challenge.Team2CurrentScore += scoreToBeAdded;
                challenge.Team1MaxPossibleScore -= scoreToBeAdded;

                switch (difficulty.ToLower())
                {
                    case "easy": challenge.Problem1SolvedByTeam = 2; break;
                    case "medium1": challenge.Problem2SolvedByTeam = 2; break;
                    case "medium2": challenge.Problem3SolvedByTeam = 2; break;
                    case "hard": challenge.Problem4SolvedByTeam = 2; break;
                }
            }

            await _teamChallengeRepo.SaveChangesAsync();

            string difficultyDisplay = difficulty.ToLower() switch
            {
                "medium1" => "Medium 1",
                "medium2" => "Medium 2",
                _ => char.ToUpper(difficulty[0]) + difficulty.Substring(1).ToLower()
            };

            string emoji = difficulty.ToLower() switch
            {
                "easy" => "🟢",
                "medium1" => "🟡",
                "medium2" => "🟡",
                "hard" => "🔴",
                _ => "⚪"
            };

            // Get the solver's mention
            var solverUser = teams.First().Users.FirstOrDefault(u => u.LeetCodeUsername == firstSolver)
                          ?? teams.Last().Users.FirstOrDefault(u => u.LeetCodeUsername == firstSolver);

            string solverMention = solverUser?.Mention ?? firstSolver;

            // Update the embed with new scores and disabled button
            await UpdateChallengeEmbed(component, challenge);

            // Notify everyone in the thread
            await threadChannel.SendMessageAsync(
                $"{emoji} **{difficultyDisplay}** solved by {solverMention} " +
                $"(Team {(firstSolverTeam == 1 ? "1️⃣" : "2️⃣")}) " +
                $"**+{scoreToBeAdded} pts!**\n" +
                $"**Current Score:** Team 1: {challenge.Team1CurrentScore} | Team 2: {challenge.Team2CurrentScore}");

            // Check if challenge is finished
            if (challenge.Team1CurrentScore > challenge.Team2MaxPossibleScore ||
                challenge.Team2CurrentScore > challenge.Team1MaxPossibleScore)
            {
                challenge.EndedAt = DateTime.UtcNow;
                await _teamChallengeRepo.SaveChangesAsync();

                var resultEmbed = await BuildTeamChallengeResultEmbedAsync(challenge.Id);
                await threadChannel.SendMessageAsync(
                    $"🎊 **CHALLENGE COMPLETE!** 🎊\n" +
                    $"Congratulations to the winners!",
                    embed: resultEmbed);

                await _teamChallengeRepo.DeleteTeamChallengeAsync(challenge.Id);

                // Remove all buttons
                await component.ModifyOriginalResponseAsync(msg =>
                {
                    msg.Components = new ComponentBuilder().Build();
                });
                return;
            }
        }

        public async Task HandleJoinTeamAsync(SocketMessageComponent component, int teamNumber)
        {
            Console.WriteLine($"Team number: {teamNumber}");

            bool isVerified = await _userRepo.IsUserExistAsync(component);
            if (!isVerified)
            {
                await component.FollowupAsync("❌ You need to verify yourself first using the /identify command.", ephemeral: true);
                return;
            }

            bool isFree = await _userRepo.IsUserFreeAsync(component);
            if (!isFree)
            {
                await component.FollowupAsync("⚠️ You are already in a challenge.", ephemeral: true);
                return;
            }

            await _userRepo.LockUserAsync(component);

            var teamChallengeId = component.Message.Id;
            var teams = await _teamChallengeRepo.GetTeamsByTeamChallengeIdAsync(teamChallengeId);
            var user = await _userRepo.GetUserByIdAsync($"{component.User.Id}-{component.GuildId}");

            if (teamNumber == 1)
            {
                var firstTeam = teams.FirstOrDefault();

                if (firstTeam.Users.Count < 2)
                {
                    await _teamRepo.AddUserToTeamAsync(firstTeam.Id, user);
                    await component.FollowupAsync($"✅ {component.User.Mention} joined **Team 1**!");
                }
                else
                {
                    await component.FollowupAsync("⚠️ Team 1 is full (2/2 players)", ephemeral: true);
                    return;
                }
            }
            else if (teamNumber == 2)
            {
                var secondTeam = teams.LastOrDefault();

                if (secondTeam.Users.Count < 2)
                {
                    await _teamRepo.AddUserToTeamAsync(secondTeam.Id, user);
                    await component.FollowupAsync($"✅ {component.User.Mention} joined **Team 2**!");
                }
                else
                {
                    await component.FollowupAsync("⚠️ Team 2 is full (2/2 players)", ephemeral: true);
                    return;
                }
            }
        }
    }
}