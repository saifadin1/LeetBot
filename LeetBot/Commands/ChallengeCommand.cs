using Discord;
using Discord.WebSocket;
using LeetBot.Interfaces;
using Microsoft.Extensions.Logging;
using System.ComponentModel;
namespace LeetBot.Commands
{
    internal class ChallengeCommand : ISlashCommand
    {
        ILogger<ChallengeCommand> _logger;
        private readonly IChallengeRepo _ChallengeRepo;
        private readonly IUserRepo _userRepo;

        public bool isApiCommand { get; set; } = true;

        public ChallengeCommand(ILogger<ChallengeCommand> logger, IChallengeRepo challengeManager, IUserRepo userRepo)
        {
            _ChallengeRepo = challengeManager;
            _userRepo = userRepo;
            _logger = logger;
        }
        public SlashCommandBuilder BuildCommand()
        {
            return new SlashCommandBuilder()
                .WithName("challenge")
                .WithDescription("Challenge a user to a LeetCode problem")
                .AddOption(new SlashCommandOptionBuilder()
                    .WithName("difficulty")
                    .WithDescription("Select difficulty: easy, medium, or hard")
                    .WithType(ApplicationCommandOptionType.String)
                    .WithRequired(true)
                    .AddChoice("Easy", "easy")
                    .AddChoice("Medium", "medium")
                    .AddChoice("Hard", "hard")
                );
        }

        public async Task ExecuteAsync(SocketSlashCommand command, ISocketMessageChannel channel)
        {
            var difficulty = command.Data.Options.First().Value.ToString();
            //var titleSlug = await _leetCodeService.GetRandomProblemAsync(difficulty);
            var userId = command.User.Id;

            // Check if the user is verified
            var existingUser = await _userRepo.IsUserExist(command);
            if (existingUser == false)
            {
                await command.RespondAsync("You need to verify yourself first using the /identify command.");
                return;
            }

            // check if the user is already in a challenge
            var isChallenging = await _ChallengeRepo.IsUserChallenging(command);
            if (isChallenging)
            {
                await command.RespondAsync("You are already in a challenge.");
                return;
            }

            // rich embed
            var embed = new EmbedBuilder()
                .WithColor(Color.Blue)
                .WithDescription($"⚡ {command.User.Mention} is calling out a challenger for a {difficulty} duel!")
                .WithFooter("Click the button to accept the challenge.");

            // button
            var components = new ComponentBuilder()
                .WithButton($"Accept", "join_btn", ButtonStyle.Primary)
                .WithButton("Leave", "leave_btn", ButtonStyle.Danger);

            try
            {
                await command.DeferAsync();

                var myChannel = (SocketTextChannel)channel;

                var thread = await myChannel.CreateThreadAsync(
                    name: $"Challenge from {command.User.Username}",
                    autoArchiveDuration: ThreadArchiveDuration.OneHour
                );

                 var message = await thread.SendMessageAsync(
                    embed: embed.Build(),
                    components: components.Build()
                );


                await command.FollowupAsync($"Challenge created ✅", ephemeral: true);
                await Task.Delay(1500);
                await command.DeleteOriginalResponseAsync();

                await _ChallengeRepo.CreateChallengAsync(command, message, difficulty);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error responding to command");
                await command.FollowupAsync("An error occurred while sending the challenge.");
                return;
            }   
        }
    }
}
