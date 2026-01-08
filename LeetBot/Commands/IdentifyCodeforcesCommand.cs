using Discord;
using Discord.WebSocket;
using LeetBot.Data;
using LeetBot.Helpers;
using LeetBot.Interfaces;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace LeetBot.Commands
{
    internal class IdentifyCodeforcesCommand : ISlashCommand
    {
        private readonly ICodeforcesService _codeforcesService;
        private readonly ILogger<IdentifyCodeforcesCommand> _logger;
        private readonly IUserRepo _userRepo;

        public bool isApiCommand { get; set; } = true;

        public IdentifyCodeforcesCommand(ICodeforcesService codeforcesService, IUserRepo userRepo, ILogger<IdentifyCodeforcesCommand> logger)
        {
            _codeforcesService = codeforcesService;
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
                .WithName("identify-cf")
                .WithDescription("Verify your Codeforces account")
                .AddOption("handle", ApplicationCommandOptionType.String, "Your Codeforces handle", isRequired: true);

            return command;
        }

        public async Task ExecuteAsync(SocketSlashCommand command, ISocketMessageChannel channel)
        {
            await command.DeferAsync();

            var codeforcesHandle = command.Data.Options
                .FirstOrDefault(x => x.Name == "handle")?.Value?.ToString();

            if (string.IsNullOrWhiteSpace(codeforcesHandle))
            {
                await command.FollowupAsync("Invalid or missing Codeforces handle.");
                return;
            }

            var randomId = GenerateRandomId(4);
            var isExistingUser = await _userRepo.IsUserExistAsync(command);

            if (isExistingUser)
            {
                var user = await _userRepo.GetUserByIdAsync(TextProcessor.UserId(command.User.Id, command.GuildId));
                if (!string.IsNullOrEmpty(user.CodeforcesHandle))
                {
                    await command.FollowupAsync("You already have a verified Codeforces account.\n" +
                        $"If you want to change it to the new handle, please place this code: `{randomId}` in your Codeforces **First Name** field.");
                }
                else
                {
                    await command.FollowupAsync($"Please set your Codeforces **First Name** to this code: `{randomId}` within 1 minute.");
                }
            }
            else
            {
                await command.FollowupAsync($"Please set your Codeforces **First Name** to this code: `{randomId}` within 1 minute.\n" +
                    "Go to: https://codeforces.com/settings/social");
            }

            var timeout = TimeSpan.FromMinutes(6);
            var delay = TimeSpan.FromSeconds(5);
            var stopwatch = Stopwatch.StartNew();

            while (stopwatch.Elapsed < timeout)
            {
                try
                {
                    var userInfo = await _codeforcesService.GetUserInfoAsync(codeforcesHandle);
                    Console.WriteLine(userInfo.FirstName);

                    if (userInfo.FirstName?.Trim() == randomId)
                    {
                        // Verification successful
                        var user = isExistingUser
                            ? await _userRepo.GetUserByIdAsync(TextProcessor.UserId(command.User.Id, command.GuildId))
                            : await _userRepo.CreateUserAsync(command);

                        user.CodeforcesHandle = codeforcesHandle;
                        user.CodeforcesRating = userInfo.Rating;
                        user.CodeforcesRank = userInfo.Rank;
                        user.CodeforcesVerifiedAt = DateTime.UtcNow;

                        await _userRepo.SaveChangesAsync();

                        // Assign role based on rating
                        await AssignCodeforcesRoleAsync(command, userInfo.Rating, userInfo.Rank);

                        await command.FollowupAsync(
                            $"✅ **Codeforces account verified!**\n" +
                            $"Handle: `{codeforcesHandle}`\n" +
                            $"Rating: **{userInfo.Rating}** ({userInfo.Rank})\n" +
                            $"Role assigned based on your rank!");

                        return;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error fetching Codeforces data for handle: {Handle}", codeforcesHandle);
                    await command.FollowupAsync($"❌ Invalid Codeforces handle or API error.");
                    return;
                }

                await Task.Delay(delay);
            }

            await command.FollowupAsync($"⏱️ Verification timed out: `{codeforcesHandle}` did not set their first name in time.");
        }

        private async Task AssignCodeforcesRoleAsync(SocketSlashCommand command, int rating, string rank)
        {
            try
            {
                var guild = (command.Channel as SocketGuildChannel)?.Guild;
                if (guild == null) return;

                var user = command.User as SocketGuildUser;
                if (user == null) return;

                // Remove all existing Codeforces rank roles
                var cfRoles = new[] { "Newbie", "Pupil", "Specialist", "Expert", "Candidate Master",
                                      "Master", "International Master", "Grandmaster",
                                      "International Grandmaster", "Legendary Grandmaster" };

                foreach (var roleName in cfRoles)
                {
                    var existingRole = guild.Roles.FirstOrDefault(r => r.Name == roleName);
                    if (existingRole != null && user.Roles.Contains(existingRole))
                    {
                        await user.RemoveRoleAsync(existingRole);
                    }
                }

                // Assign new role based on rank
                var targetRoleName = GetRoleNameFromRank(rank);
                IRole targetRole = guild.Roles.FirstOrDefault(r => r.Name == targetRoleName);

                if (targetRole == null)
                {
                    // Create role with appropriate color if it doesn't exist
                    var color = GetColorForRank(rank);
                    targetRole = await guild.CreateRoleAsync(targetRoleName, color: color, isMentionable: false);
                }

                await user.AddRoleAsync(targetRole);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error assigning Codeforces role");
            }
        }

        private string GetRoleNameFromRank(string rank)
        {
            return rank switch
            {
                "newbie" => "Newbie",
                "pupil" => "Pupil",
                "specialist" => "Specialist",
                "expert" => "Expert",
                "candidate master" => "Candidate Master",
                "master" => "Master",
                "international master" => "International Master",
                "grandmaster" => "Grandmaster",
                "international grandmaster" => "International Grandmaster",
                "legendary grandmaster" => "Legendary Grandmaster",
                _ => "Unrated"
            };
        }

        private Color GetColorForRank(string rank)
        {
            return rank switch
            {
                "newbie" => new Color(128, 128, 128),           // Gray
                "pupil" => new Color(0, 128, 0),                // Green
                "specialist" => new Color(3, 168, 158),         // Cyan
                "expert" => new Color(0, 0, 255),               // Blue
                "candidate master" => new Color(170, 0, 170),   // Violet
                "master" => new Color(255, 140, 0),             // Orange
                "international master" => new Color(255, 140, 0), // Orange
                "grandmaster" => new Color(255, 0, 0),          // Red
                "international grandmaster" => new Color(255, 0, 0), // Red
                "legendary grandmaster" => new Color(170, 0, 0), // Dark Red
                _ => new Color(128, 128, 128)                   // Default Gray
            };
        }
    }
}