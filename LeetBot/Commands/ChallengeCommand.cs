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
                )
                .AddOption(new SlashCommandOptionBuilder()
                    .WithName("topic")
                    .WithDescription("select a topic optionally")
                    .WithType(ApplicationCommandOptionType.String)
                    .WithRequired(false)
                    // max number of choices is 25
                    .AddChoice("Array", "array")
                    .AddChoice("String", "string")
                    .AddChoice("Dynamic Programming", "dynamic-programming")
                    .AddChoice("Binary Search", "binary-search")
                    .AddChoice("Backtracking", "backtracking")
                    .AddChoice("Greedy", "greedy")
                    .AddChoice("Graph", "graph")
                    .AddChoice("Tree", "tree")
                    .AddChoice("Linked List", "linked-list")
                    .AddChoice("Heap", "heap")
                    .AddChoice("Hash Table", "hash-table")
                    .AddChoice("Recursion", "recursion")
                    .AddChoice("Sorting", "sorting")
                    .AddChoice("Bit Manipulation", "bit-manipulation")
                    .AddChoice("Math", "math")
                    .AddChoice("numbertheory", "number-theory")
                    .AddChoice("Database", "database")
                    .AddChoice("Shortest path", "shortest-path")
                    .AddChoice("Prefix sum", "prefix-sum")
                    .AddChoice("Sliding Window", "sliding-window")
                    .AddChoice("Two Pointers", "two-pointers")
                );
        }

        public async Task ExecuteAsync(SocketSlashCommand command, ISocketMessageChannel channel)
        {
            var difficulty = command.Data.Options.First().Value.ToString();
            var topic = command.Data.Options.FirstOrDefault(x => x.Name == "topic")?.Value?.ToString();

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
                .WithDescription($"⚡ {command.User.Mention} is calling out a challenger for a **{difficulty} {topic ?? "random"}** duel!")
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

                await _ChallengeRepo.CreateChallengAsync(command, message, difficulty, topic);
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
