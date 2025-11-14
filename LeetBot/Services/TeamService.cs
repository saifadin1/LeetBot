using Discord;
using Discord.WebSocket;
using LeetBot.ComponentHandlers.TeamChallenge.Joins;
using LeetBot.DTOs;
using LeetBot.Interfaces;
using LeetBot.Models;
using LeetBot.Repositories;
using Microsoft.Extensions.Logging;
using System.Drawing;
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
                title = "Team 1 is Victorious! 🏆";
                color = new Color(88, 101, 242);
                description = $"Team 1 wins with a final score of **{team1Score}** to **{team2Score}**!";
            }
            else if (team2Score > team1Score)
            {
                title = "Team 2 is Victorious! 🏆";
                color = new Color(237, 66, 69);
                description = $"Team 2 wins with a final score of **{team2Score}** to **{team1Score}**!";
            }
            else
            {
                title = "It's a Draw! 🤝";
                color = Color.LightGrey;
                description = $"Both teams tied with a score of **{team1Score}**!";
            }

            var embedBuilder = new EmbedBuilder()
                .WithTitle(title)
                .WithDescription(description)
                .WithColor(color)
                .WithTimestamp(DateTimeOffset.UtcNow)
                .WithFooter("Challenge complete");

            embedBuilder.AddField("Easy Problem", challenge.EasyProblemTitleSlug ?? "Not Set", inline: true);
            embedBuilder.AddField("Medium Problem", challenge.MediumProblemTitleSlug ?? "Not Set", inline: true);
            embedBuilder.AddField("Hard Problem", challenge.HardProblemTitleSlug ?? "Not Set", inline: true);


            if (challenge.Teams != null && challenge.Teams.Count >= 2)
            {
                var teams = challenge.Teams.ToList();
                // Assuming the order is consistent (e.g., Team 1 is first in list)
                var team1Users = teams[0].Users;
                var team2Users = teams[1].Users;

                var team1Members = team1Users.Any() ? string.Join("\n", team1Users.Select(u => u.Mention)) : "No members";
                var team2Members = team2Users.Any() ? string.Join("\n", team2Users.Select(u => u.Mention)) : "No members";

                embedBuilder.AddField("\u200B", "\u200B"); // Blank field for spacing

                embedBuilder.AddField("Team 1 Members", team1Members, inline: true);
                embedBuilder.AddField("Team 1 Score", $"**{challenge.Team1CurrentScore}**", inline: true);
                embedBuilder.AddField("\u200B", "\u200B", inline: true); // invisible spacer

                embedBuilder.AddField("Team 2 Members", team2Members, inline: true);
                embedBuilder.AddField("Team 2 Score", $"**{challenge.Team2CurrentScore}**", inline: true);
                embedBuilder.AddField("\u200B", "\u200B", inline: true); // invisible spacer
            }
            else
            {
                _logger.LogWarning("Challenge {ChallengeId} did not have 2 teams with users included.", challengeId);
                embedBuilder.AddField("\u200B", "\u200B"); // Blank field for spacing
                embedBuilder.AddField("Team Scores", $"Team 1: **{challenge.Team1CurrentScore}**\nTeam 2: **{challenge.Team2CurrentScore}**");
                embedBuilder.AddField("Team Info", "Could not retrieve full team member information.");
            }



            return embedBuilder.Build();
        }

        public async Task HandleDifficultyButton(SocketMessageComponent component, SocketThreadChannel threadChannel, string difficulty)
        {

            // validation (verified, free)
            bool isVerified = await _userRepo.IsUserExistAsync(component);
            if (!isVerified)
            {
                await component.FollowupAsync("You need to verify yourself first using the /identify command.");
                return;
            }
            //bool isFree = await _userRepo.IsUserFreeAsync(component);
            //if (!isFree)
            //{
            //    await component.FollowupAsync("You are already in a challenge.", ephemeral: true);
            //    return;
            //}

            // get all users last submissions
            var challenge = await _teamChallengeRepo.GetTeamChallengeByIdAsync(component.Message.Id);

            if (challenge == null)
            {
                _logger.LogError("Challenge not found.");
                await component.FollowupAsync("Challenge not found.", ephemeral: true);
                return;
            }

            string problemSlug;
            switch (difficulty.ToLower())
            {
                case "easy":
                    problemSlug = challenge.EasyProblemTitleSlug;
                    break;
                case "medium":
                    problemSlug = challenge.MediumProblemTitleSlug;
                    break;
                case "hard":
                    problemSlug = challenge.HardProblemTitleSlug;
                    break;
                default:
                    await component.FollowupAsync("Invalid difficulty.", ephemeral: true);
                    return;
            }

            // make sure this problem not solved yet
            if (challenge.Problem3SolvedByTeam != 0)
            {
                await component.FollowupAsync("This problem is already solved by one of the teams", ephemeral: true);
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
                await component.FollowupAsync("No submissions found for this problem, please click this button ONLY when you solve the problem", ephemeral: true);
                return;
            }

            var firstSolver = allSubmissions
                .FirstOrDefault()?
                .LeetCodeUsername;


            // 1. update scores - take care about race condition !!
            // 2. update the message 
            // 3. respond



            int firstSolverTeam;
            int scoreToBeAdded;
            switch (difficulty.ToLower())
            {
                case "easy":
                    scoreToBeAdded = 100;
                    break;
                case "medium":
                    scoreToBeAdded = 200;
                    break;
                case "hard":
                    scoreToBeAdded = 400;
                    break;
                default:
                    scoreToBeAdded = 0;
                    break;
            }

            if (teams.First().Users.Any(x => x.LeetCodeUsername == firstSolver))
            {
                firstSolverTeam = 1;
                challenge.Team1CurrentScore += scoreToBeAdded;
                challenge.Team2MaxPossibleScore -= scoreToBeAdded;
                switch (difficulty.ToLower())
                {
                    case "easy":
                        challenge.Problem1SolvedByTeam = 1;
                        break;
                    case "medium":
                        challenge.Problem2SolvedByTeam = 1;
                        break;
                    case "hard":
                        challenge.Problem3SolvedByTeam = 1;
                        break;
                }
            }
            else
            {
                firstSolverTeam = 2;
                challenge.Team1CurrentScore += scoreToBeAdded;
                challenge.Team2MaxPossibleScore -= scoreToBeAdded;
                switch (difficulty.ToLower())
                {
                    case "easy":
                        challenge.Problem1SolvedByTeam = 2;
                        break;
                    case "medium":
                        challenge.Problem2SolvedByTeam = 2;
                        break;
                    case "hard":
                        challenge.Problem3SolvedByTeam = 2;
                        break;
                }
            }

            await component.FollowupAsync($"the {difficulty.ToUpper()} problem solved by team {(firstSolverTeam == 1 ? "1️⃣ " : "2️⃣ ")}");

            // check if the challenge is finished
            if (challenge.Team1CurrentScore > challenge.Team2MaxPossibleScore)
            {
                await threadChannel.SendMessageAsync($"Team 1 wins with score: {challenge.Team1CurrentScore} - {challenge.Team2CurrentScore}");
                await _teamChallengeRepo.DeleteTeamChallengeAsync(challenge.Id);
                await component.ModifyOriginalResponseAsync(msg =>
                {
                    msg.Embed = new EmbedBuilder()
                        .WithTitle("Challenge finished")
                        .WithDescription($"Team 1 wins with score: {challenge.Team1CurrentScore} ")
                        .WithColor(Color.Green)
                        .Build();
                });
                return;
            }
            else if (challenge.Team2CurrentScore > challenge.Team1MaxPossibleScore)
            {
                await threadChannel.SendMessageAsync($"Team 2 wins with score: {challenge.Team2CurrentScore} - {challenge.Team1CurrentScore}");
                await _teamChallengeRepo.DeleteTeamChallengeAsync(challenge.Id);
                await component.ModifyOriginalResponseAsync(msg =>
                {
                    msg.Embed = new EmbedBuilder()
                        .WithTitle("Challenge finished")
                        .WithDescription($"Team 1 wins with score: {challenge.Team1CurrentScore} ")
                        .WithColor(Color.Green)
                        .Build();
                });
                return;
            }
        }

        public async Task HandleJoinTeamAsync(SocketMessageComponent component, int teamNumber)
        {
            Console.WriteLine($"Team number: {teamNumber}");


            // validation (verified, free)
            bool isVerified = await _userRepo.IsUserExistAsync(component);
            if (!isVerified)
            {
                await component.FollowupAsync("You need to verify yourself first using the /identify command.");
                return;
            }
            bool isFree = await _userRepo.IsUserFreeAsync(component);
            if (!isFree)
            {
                await component.FollowupAsync("You are already in a challenge.", ephemeral: true);
                return;
            }
            await _userRepo.LockUserAsync(component);

            var teamChallengeId = component.Message.Id;

            var teams = await _teamChallengeRepo.GetTeamsByTeamChallengeIdAsync(teamChallengeId);
            var user = await _userRepo.GetUserByIdAsync($"{component.User.Id}-{component.GuildId}");

            if (teamNumber == 1)
            {

                var firstTeam = teams.FirstOrDefault();
                var firstTeamUsers = firstTeam.Users.ToList();

                if (firstTeam.Users.Count < 2)
                {
                    await _teamRepo.AddUserToTeamAsync(firstTeam.Id, user);

                    await component.FollowupAsync($"{component.User.Mention} has joined team: {teamNumber} ✅");
                }
                else
                {
                    await component.FollowupAsync("Team 1 is full", ephemeral: true);
                    return;
                }
            }
            else if (teamNumber == 2)
            {
                var secondTeam = teams.LastOrDefault();
                var secondTeamUsers = secondTeam.Users.ToList();
                if (secondTeam.Users.Count < 2)
                {
                    await _teamRepo.AddUserToTeamAsync(secondTeam.Id, user);
                    await component.FollowupAsync($"{component.User.Mention} has joined team: {teamNumber} ✅");
                }
                else
                {
                    await component.FollowupAsync("Team 2 is full", ephemeral: true);
                    return;
                }
            }
        }


    }
}
