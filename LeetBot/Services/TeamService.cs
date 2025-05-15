using Discord.WebSocket;
using LeetBot.ComponentHandlers;
using LeetBot.Interfaces;
using Microsoft.Extensions.Logging;

namespace LeetBot.Services
{
    public class TeamService : ITeamService
    {
        private readonly ITeamChallengeRepo _teamChallengeRepo;
        private readonly IUserRepo _userRepo;
        private readonly ITeamRepo _teamRepo;
        private readonly ILogger<JoinTeam1BtnHandler> _logger;

        public TeamService(ITeamChallengeRepo teamChallengeRepo,
            IUserRepo userRepo,
            ILogger<JoinTeam1BtnHandler> logger,
            ITeamRepo teamRepo)
        {
            _teamChallengeRepo = teamChallengeRepo;
            _userRepo = userRepo;
            _logger = logger;
            _teamRepo = teamRepo;
        }

        public async Task HandleJoinTeamAsync(SocketMessageComponent component, int teamNumber)
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
