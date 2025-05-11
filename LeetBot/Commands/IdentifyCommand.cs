using Discord;
using Discord.WebSocket;
using LeetBot.Data;
using LeetBot.Interfaces;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace LeetBot.Commands
{
    internal class IdentifyCommand : ISlashCommand
    {
        private readonly ILeetCodeService _leetCodeService;
        private readonly ILogger<IdentifyCommand> _logger;
        private readonly IUserRepo _userRepo;

        public bool isApiCommand { get; set; } = true;

        public IdentifyCommand(ILeetCodeService leetCodeService, IUserRepo userRepo, ILogger<IdentifyCommand> logger)
        {
            _leetCodeService = leetCodeService;
            _userRepo = userRepo;
            _logger = logger;
        }

        public string GenerateRandomId(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            var random = new Random();
            var randomId = new char[length];
            for (int i = 0; i < length; i++)
            {
                randomId[i] = chars[random.Next(chars.Length)];
            }
            return new string(randomId);
        }

        public SlashCommandBuilder BuildCommand()
        {
            var command = new SlashCommandBuilder()
                .WithName("identify")
                .WithDescription("Identify yourself")
                .AddOption("leetcode_id", ApplicationCommandOptionType.String, "Your LeetCode id", isRequired: true);

            return command;
        }

        public async Task ExecuteAsync(SocketSlashCommand command, ISocketMessageChannel channel)
        {
            await command.DeferAsync();

            var LeetCodeUsername = command.Data.Options 
                .FirstOrDefault(x => x.Name == "leetcode_id")?.Value?.ToString();

            if (string.IsNullOrWhiteSpace(LeetCodeUsername))
            {
                await command.FollowupAsync("Invalid or missing LeetCode_id.");
                return;
            }

            var randomId = GenerateRandomId(4);

            var isExistingUser = await _userRepo.IsUserExistAsync(command);
            if (isExistingUser)
            {
                await command.FollowupAsync("You are already verified.");
                return;
            }

            await command.FollowupAsync($"Please set your LeetCode real name to this code: `{randomId}` within 1 minute.");

            var timeout = TimeSpan.FromMinutes(1);
            var delay = TimeSpan.FromSeconds(5);
            var stopwatch = Stopwatch.StartNew();

            while (stopwatch.Elapsed < timeout)
            {
                string realName = null;
                try
                {
                    realName = await _leetCodeService.GetUserRealNameAsync(LeetCodeUsername);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error fetching real name from LeetCode");
                    await command.FollowupAsync($"invalid LeetCode_id");
                    return;
                }

                if (realName?.Trim() == randomId)
                {
                    var user = await _userRepo.CreateUserAsync(command);
                    user.LeetCodeUsername = LeetCodeUsername;
                    await _userRepo.SaveChangesAsync();
                    await command.FollowupAsync($"{LeetCodeUsername} has been verified successfully!");
                    return;
                }

                await Task.Delay(delay);
            }

            await command.FollowupAsync($"Verification failed: `{LeetCodeUsername}` did not set their real name in time.");
        }

    }
}
