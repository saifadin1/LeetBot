using Discord;
using Discord.WebSocket;
using LeetBot.Helpers;
using LeetBot.Interfaces;
using Microsoft.Extensions.Logging;

namespace LeetBot.ComponentHandlers.TeamChallenge
{
    public class StartTeamBtnHandler : IComponentHandler
    {
        private readonly ILeetCodeService _leetCodeService;
        private readonly ITeamChallengeRepo _teamChallengeRepo;
        private readonly IUserRepo _userRepo;
        private readonly ITeamRepo _teamRepo;
        private readonly ILogger<StartTeamBtnHandler> _logger;
        private readonly ITeamService _teamService;

        public StartTeamBtnHandler(ILeetCodeService leetCodeService,
            ITeamChallengeRepo teamChallengeRepo,
            IUserRepo userRepo,
            ILogger<StartTeamBtnHandler> logger,
            ITeamRepo teamRepo,
            ITeamService teamService)
        {
            _leetCodeService = leetCodeService;
            _teamChallengeRepo = teamChallengeRepo;
            _userRepo = userRepo;
            _logger = logger;
            _teamRepo = teamRepo;
            _teamService = teamService;
        }

        public string CustomId => "startTeamBtn";

        public async Task ExecuteAsync(SocketMessageComponent component, SocketThreadChannel threadChannel)
        {
            await component.DeferAsync();

            // validation - this user is the creator of the thread FirstTeam.user.First
            var challengeId = component.Message.Id;
            var teams = await _teamRepo.GetTeamsByChallengeIdAsync((long)challengeId);
            //var creator = teams
            //    .FirstOrDefault()?
            //    .Users
            //    .FirstOrDefault();

            // validation - user is the creator of the thread

            //if (creator is null || creator.Id != TextProcessor.UserId(component.User.Id, component.GuildId))
            //{
            //    await component.FollowupAsync("Only the creator of the challenge can start it", ephemeral: true);
            //    return;
            //}

            // validation - both teams have 2 users
            if (teams.First().Users.Count != 2 && teams.Last().Users.Count != 2)
            {
                await component.FollowupAsync("Both teams must have 2 users to start the challenge", ephemeral: true);
                return;
            }

            
            var easyProblem = await _leetCodeService.GetRandomProblemAsync("easy", null);
            var mediumProblem = await _leetCodeService.GetRandomProblemAsync("medium", null);
            var hardProblem = await _leetCodeService.GetRandomProblemAsync("hard", null);

            var challenge = await _teamChallengeRepo.GetTeamChallengeByIdAsync((long)challengeId);

            challenge.EasyProblemTitleSlug = easyProblem;
            challenge.MediumProblemTitleSlug = mediumProblem;
            challenge.HardProblemTitleSlug = hardProblem;

            await _teamChallengeRepo.SaveChangesAsync();

            // modify the message 
            var embed = new EmbedBuilder()
                 .WithTitle("Team Challenge started!")
                 .WithDescription($"**Easy:** {TextProcessor.ProblemLink(easyProblem)}\n**Medium:** {TextProcessor.ProblemLink(mediumProblem)}\n**Hard:** {TextProcessor.ProblemLink(hardProblem)}")
                 .WithColor(Color.Blue);

            var components = new ComponentBuilder()
                .WithButton("Easy", "teamEasy", ButtonStyle.Primary)
                .WithButton("Medium", "teamMedium", ButtonStyle.Primary)
                .WithButton("Hard", "teamHard", ButtonStyle.Primary);

            await component.ModifyOriginalResponseAsync(msg =>
            {
                msg.Embed = embed.Build();
                msg.Components = components.Build();
            });
        }
    }
}
