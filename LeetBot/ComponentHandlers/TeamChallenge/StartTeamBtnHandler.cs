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

            var loadingEmbed = new EmbedBuilder()
                 .WithTitle("Starting Team Challenge...")
                 .WithDescription("Fetching problems, please wait...")
                 .WithColor(Color.Orange);

            await component.ModifyOriginalResponseAsync(msg =>
            {
                msg.Embed = loadingEmbed.Build();
                msg.Components = new ComponentBuilder().Build();
            });

            var challengeId = component.Message.Id;
            var teams = await _teamRepo.GetTeamsByChallengeIdAsync(challengeId);

            // Fetch all problems
            var easyProblem = await _leetCodeService.GetRandomProblemAsync("easy", null);
            var mediumProblem1 = await _leetCodeService.GetRandomProblemAsync("medium", null);
            var mediumProblem2 = await _leetCodeService.GetRandomProblemAsync("medium", null);
            var hardProblem = await _leetCodeService.GetRandomProblemAsync("hard", null);

            var challenge = await _teamChallengeRepo.GetTeamChallengeByIdAsync(challengeId);

            challenge.EasyProblemTitleSlug = easyProblem;
            challenge.MediumProblem1TitleSlug = mediumProblem1;
            challenge.MediumProblem2TitleSlug = mediumProblem2;
            while (mediumProblem2 == mediumProblem1)
            {
                mediumProblem2 = await _leetCodeService.GetRandomProblemAsync("medium", null);
            }
            challenge.HardProblemTitleSlug = hardProblem;
            challenge.IsActive = true;

            await _teamChallengeRepo.SaveChangesAsync();

            var embed = new EmbedBuilder()
                .WithTitle("Team Challenge Started! 🎯")
                .WithDescription(
                    $"**Easy (100 pts):** [{easyProblem}]({TextProcessor.ProblemLink(easyProblem)})\n" +
                    $"**Medium 1 (200 pts):** [{mediumProblem1}]({TextProcessor.ProblemLink(mediumProblem1)})\n" +
                    $"**Medium 2 (200 pts):** [{mediumProblem2}]({TextProcessor.ProblemLink(mediumProblem2)})\n" +
                    $"**Hard (400 pts):** [{hardProblem}]({TextProcessor.ProblemLink(hardProblem)})\n\n" +
                    $"**Total Points:** 900")
                .WithColor(Color.Blue)
                .WithFooter("Click a button when you complete a problem!");

            var components = new ComponentBuilder()
                .WithButton("Easy", "teamEasy", ButtonStyle.Success)
                .WithButton("Medium 1", "teamMedium1", ButtonStyle.Primary)
                .WithButton("Medium 2", "teamMedium2", ButtonStyle.Primary)
                .WithButton("Hard", "teamHard", ButtonStyle.Danger);

            await component.ModifyOriginalResponseAsync(msg =>
            {
                msg.Embed = embed.Build();
                msg.Components = components.Build();
            });
        }
    }
}