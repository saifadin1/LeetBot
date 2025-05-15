using Discord;
using Discord.WebSocket;
using LeetBot.ComponentHandlers.TeamChallenge.Joins;
using LeetBot.DTOs;
using LeetBot.Interfaces;
using LeetBot.Models;
using Microsoft.Extensions.Logging;

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

        public async Task HandleDifficultyButton(SocketMessageComponent component, SocketThreadChannel threadChannel, string difficulty)
        {
            await component.DeferAsync();

            // validation (verified, free)
            bool isVerified = await _userRepo.IsUserExistAsync(component);
            if (!isVerified)
            {
                await component.RespondAsync("You need to verify yourself first using the /identify command.");
                return;
            }
            bool isFree = await _userRepo.IsUserFreeAsync(component);
            if (!isFree)
            {
                await component.RespondAsync("You are already in a challenge.", ephemeral: true);
                return;
            }

            // get all users last submissions
            var challenge = await _teamChallengeRepo.GetTeamChallengeByIdAsync((long)component.Message.Id);

            if (challenge == null)
            {
                _logger.LogError("Challenge not found.");
                await component.RespondAsync("Challenge not found.", ephemeral: true);
                return;
            }

            // make sure this problem not solved yet
            if (challenge.Problem3SolvedByTeam != 0)
            {
                await component.FollowupAsync("This problem is already solved by one of the teams", ephemeral: true);
                return;
            }

            var problemSlug = challenge.HardProblemTitleSlug;
            var teams = challenge.Teams;
            var allSubmissions = new List<UserLastSubmissionDTO>();
            foreach (var user in (IEnumerable<User>)teams.First())
            {
                var lastSubmission = await _leet.GetUserSubmissionsAsync(user.LeetCodeUsername);
                if (lastSubmission != null && lastSubmission.TitleSlug == problemSlug)
                {
                    allSubmissions.Add(lastSubmission);
                }
            }
            foreach (var user in (IEnumerable<User>)teams.Last())
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
                    scoreToBeAdded = 300;
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
                challenge.Problem3SolvedByTeam = 1;
            }
            else
            {
                firstSolverTeam = 2;
                challenge.Team1CurrentScore += scoreToBeAdded;
                challenge.Team2MaxPossibleScore -= scoreToBeAdded;
                challenge.Problem3SolvedByTeam = 2;
            }

            await component.FollowupAsync($"the {difficulty.ToUpper()} problem solved by team {(firstSolverTeam == 1 ? "1️⃣ " : "2️⃣ ")}");

            // check if the challenge is finished
            if (challenge.Team1CurrentScore >= challenge.Team2MaxPossibleScore)
            {
                await threadChannel.SendMessageAsync($"Team 1 wins with score: {challenge.Team1CurrentScore} - {challenge.Team2MaxPossibleScore}");
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
            else if (challenge.Team2CurrentScore >= challenge.Team1MaxPossibleScore)
            {
                await threadChannel.SendMessageAsync($"Team 2 wins with score: {challenge.Team2CurrentScore} - {challenge.Team1MaxPossibleScore}");
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

            var teams = await _teamChallengeRepo.GetTeamsByTeamChallengeIdAsync((long)teamChallengeId);
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
