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
                 .WithTitle("⏳ Starting Team Challenge...")
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

            // Ensure medium problems are different
            while (mediumProblem2 == mediumProblem1)
            {
                mediumProblem2 = await _leetCodeService.GetRandomProblemAsync("medium", null);
            }

            var hardProblem = await _leetCodeService.GetRandomProblemAsync("hard", null);

            var challenge = await _teamChallengeRepo.GetTeamChallengeByIdAsync(challengeId);

            challenge.EasyProblemTitleSlug = easyProblem;
            challenge.MediumProblem1TitleSlug = mediumProblem1;
            challenge.MediumProblem2TitleSlug = mediumProblem2;
            challenge.HardProblemTitleSlug = hardProblem;
            challenge.IsActive = true;
            challenge.StartedAt = DateTime.UtcNow;

            await _teamChallengeRepo.SaveChangesAsync();

            // Get team members for display
            var team1 = teams.FirstOrDefault();
            var team2 = teams.LastOrDefault();

            var team1Members = team1?.Users.Any() == true
                ? string.Join(", ", team1.Users.Select(u => u.Mention))
                : "*Waiting for players...*";
            var team2Members = team2?.Users.Any() == true
                ? string.Join(", ", team2.Users.Select(u => u.Mention))
                : "*Waiting for players...*";

            var embed = new EmbedBuilder()
                .WithTitle("🎯 Team Challenge - IN PROGRESS")
                .WithDescription("First team to solve wins the points!")
                .WithColor(new Color(88, 101, 242))
                .WithCurrentTimestamp();

            // Add team information
            embed.AddField("👥 Team 1", team1Members, inline: true);
            embed.AddField("👥 Team 2", team2Members, inline: true);
            embed.AddField("\u200B", "\u200B", inline: false); // Spacer

            // Add problems with emojis
            embed.AddField("📝 Problems",
                $"🟢 **Easy (100 pts):** [{easyProblem}]({TextProcessor.ProblemLink(easyProblem)})\n" +
                $"🟡 **Medium 1 (200 pts):** [{mediumProblem1}]({TextProcessor.ProblemLink(mediumProblem1)})\n" +
                $"🟡 **Medium 2 (200 pts):** [{mediumProblem2}]({TextProcessor.ProblemLink(mediumProblem2)})\n" +
                $"🔴 **Hard (400 pts):** [{hardProblem}]({TextProcessor.ProblemLink(hardProblem)})",
                inline: false);

            // Add current scores
            embed.AddField("📊 Current Score",
                $"Team 1: **{challenge.Team1CurrentScore}** pts | Team 2: **{challenge.Team2CurrentScore}** pts",
                inline: false);

            embed.WithFooter("Click a button after solving a problem • Good luck! 🍀");

            var components = new ComponentBuilder()
                .WithButton("Easy", "teamEasy", ButtonStyle.Success, new Emoji("🟢"))
                .WithButton("Medium 1", "teamMedium1", ButtonStyle.Primary, new Emoji("🟡"))
                .WithButton("Medium 2", "teamMedium2", ButtonStyle.Primary, new Emoji("🟡"))
                .WithButton("Hard", "teamHard", ButtonStyle.Danger, new Emoji("🔴"));

            await component.ModifyOriginalResponseAsync(msg =>
            {
                msg.Embed = embed.Build();
                msg.Components = components.Build();
            });

            // Notify all players that the challenge has started
            await threadChannel.SendMessageAsync(
                $"🎮 Challenge started! {team1Members} vs {team2Members}\n" +
                $"Total points available: **900**");
        }
    }
}