using Discord;
using Discord.WebSocket;
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

        public IdentifyCodeforcesCommand(ICodeforcesService codeforcesService, IUserRepo userRepo,
            ILogger<IdentifyCodeforcesCommand> logger)
        {
            _codeforcesService = codeforcesService;
            _userRepo = userRepo;
            _logger = logger;
        }

        private string GenerateRandomCode(int length = 6)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            var random = new Random();
            return new string(Enumerable.Range(0, length)
                .Select(_ => chars[random.Next(chars.Length)])
                .ToArray());
        }

        public SlashCommandBuilder BuildCommand()
        {
            return new SlashCommandBuilder()
                .WithName("identify-cf")
                .WithDescription("Verify your Codeforces account and get ranked role")
                .AddOption("handle", ApplicationCommandOptionType.String,
                    "Your Codeforces handle", isRequired: true);
        }

        public async Task ExecuteAsync(SocketSlashCommand command, ISocketMessageChannel channel)
        {
            await command.DeferAsync();

            var codeforcesHandle = command.Data.Options
                .FirstOrDefault(x => x.Name == "handle")?.Value?.ToString()?.Trim();

            if (string.IsNullOrWhiteSpace(codeforcesHandle))
            {
                await command.FollowupAsync("❌ Invalid or missing Codeforces handle.");
                return;
            }

            CodeforcesUserInfo userInfo;
            try
            {
                userInfo = await _codeforcesService.GetUserInfoAsync(codeforcesHandle);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching Codeforces data for handle: {Handle}", codeforcesHandle);

                var errorEmbed = new EmbedBuilder()
                    .WithTitle("Handle Identify")
                    .WithDescription($"Sorry {command.User.Mention}, can you try again?")
                    .AddField("Handle", codeforcesHandle, inline: true)
                    .AddField("Status", "❌ Invalid Handle / API Error", inline: true)
                    .WithColor(Color.Red)
                    .WithThumbnailUrl(command.User.GetAvatarUrl() ?? command.User.GetDefaultAvatarUrl())
                    .WithCurrentTimestamp()
                    .Build();

                await command.FollowupAsync(embed: errorEmbed);
                return;
            }

            var isExistingUser = await _userRepo.IsUserExistAsync(command);
            var user = isExistingUser ? await _userRepo.GetUserByIdAsync(TextProcessor.UserId(command.User.Id, command.GuildId)) : null;

            var verificationCode = GenerateRandomCode(); // Used only for display in instructions
            var problemId = GetRandomProblem();          // e.g. "1677A"
            var (targetContestId, targetIndex) = ParseProblemId(problemId);

            var instructionEmbed = new EmbedBuilder()
                .WithTitle("Handle Identify")
                .WithDescription(
                    $"To verify your Codeforces handle, submit a **compilation error** to the following problem:\n\n" +
                    $"**Problem:** [{problemId}](https://codeforces.com/problemset/problem/{problemId})\n" +
                    $"**Instructions:**\n" +
                    $"1. Click the link above.\n" +
                    $"2. Submit ANY code that causes a compilation error (e.g., just `hello world`).\n" +
                    $"3. The bot will check your **last submission** automatically.\n\n" +
                    $"*We check for: Problem {problemId} + Verdict: Compilation Error*")
                .WithColor(Color.Blue)
                .WithThumbnailUrl(userInfo.Avatar ?? command.User.GetDefaultAvatarUrl())
                .WithFooter("Verification expires in 2 minutes")
                .WithCurrentTimestamp()
                .Build();

            await command.FollowupAsync(embed: instructionEmbed);

            var timeout = TimeSpan.FromMinutes(2);
            var delay = TimeSpan.FromSeconds(5);
            var stopwatch = Stopwatch.StartNew();

            while (stopwatch.Elapsed < timeout)
            {
                try
                {
                    var submissions = await _codeforcesService.GetRecentSubmissionsAsync(codeforcesHandle, 1);
                    var lastSubmission = submissions.FirstOrDefault();

                    if (lastSubmission != null)
                    {
                        if (lastSubmission.Verdict == "COMPILATION_ERROR" &&
                            lastSubmission.ContestId  == targetContestId.ToString() &&
                            lastSubmission.ProblemIndex.ToString() == targetIndex)
                        {

                            if (!isExistingUser)
                            {
                                user = await _userRepo.CreateUserAsync(command);
                            }

                            await _userRepo.UpdateUserCodeforcesAsync(command,
                                codeforcesHandle, userInfo.Rating, userInfo.Rank);

                            await AssignCodeforcesRoleAsync(command, userInfo.Rank);

                            var cfAvatar = !string.IsNullOrEmpty(userInfo.Avatar)
                                ? userInfo.Avatar
                                : command.User.GetAvatarUrl() ?? command.User.GetDefaultAvatarUrl();

                            var successEmbed = new EmbedBuilder()
                                .WithTitle("Handle Identify")
                                .WithDescription($"{command.User.Mention}, your Codeforces account has been verified!")
                                .AddField("Handle", codeforcesHandle, inline: true)
                                .AddField("Rank", $"{GetRankEmoji(userInfo.Rank)} {userInfo.Rank}", inline: true)
                                .AddField("Status", "✅ Verified", inline: true)
                                .AddField("Rating", $"**{userInfo.Rating}**", inline: true)
                                .AddField("Max Rating", $"**{userInfo.MaxRating}** ({userInfo.MaxRank})", inline: true)
                                .AddField("Role", GetRoleNameFromRank(userInfo.Rank), inline: true)
                                .WithColor(GetColorForRank(userInfo.Rank))
                                .WithThumbnailUrl(cfAvatar)
                                .WithCurrentTimestamp()
                                .Build();

                            await command.FollowupAsync(embed: successEmbed);
                            return; 
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error checking submissions for handle: {Handle}", codeforcesHandle);
                }

                await Task.Delay(delay);
            }

            var timeoutEmbed = new EmbedBuilder()
                .WithTitle("Handle Identify")
                .WithDescription($"Sorry {command.User.Mention}, verification timed out.")
                .AddField("Handle", codeforcesHandle, inline: true)
                .AddField("Reason", $"No COMPILATION_ERROR found on problem {problemId} as the last submission.", inline: false)
                .WithColor(Color.Orange)
                .WithThumbnailUrl(command.User.GetAvatarUrl() ?? command.User.GetDefaultAvatarUrl())
                .WithFooter("Please try running the command again.")
                .WithCurrentTimestamp()
                .Build();

            await command.FollowupAsync(embed: timeoutEmbed);
        }


        (int id, string index) ParseProblemId(string problemId)
        {
            var match = System.Text.RegularExpressions.Regex.Match(problemId, @"^(\d+)([A-Z]\d*)$");
            return match.Success
                ? (int.Parse(match.Groups[1].Value), match.Groups[2].Value)
                : (0, string.Empty);
        }


        private async Task AssignCodeforcesRoleAsync(SocketSlashCommand command, string rank)
        {
            try
            {
                var guild = (command.Channel as SocketGuildChannel)?.Guild;
                if (guild == null) return;

                var user = command.User as SocketGuildUser;
                if (user == null) return;

                var cfRoles = new[]
                {
                    "Newbie", "Pupil", "Specialist", "Expert",
                    "Candidate Master", "Master", "International Master",
                    "Grandmaster", "International Grandmaster", "Legendary Grandmaster",
                    "Unrated"
                };

                foreach (var roleName in cfRoles)
                {
                    var existingRole = guild.Roles.FirstOrDefault(r =>
                        r.Name.Equals(roleName, StringComparison.OrdinalIgnoreCase));

                    if (existingRole != null && user.Roles.Contains(existingRole))
                    {
                        await user.RemoveRoleAsync(existingRole);
                        await Task.Delay(100);
                    }
                }

                var targetRoleName = GetRoleNameFromRank(rank);
                IRole targetRole = guild.Roles.FirstOrDefault(r =>
                    r.Name.Equals(targetRoleName, StringComparison.OrdinalIgnoreCase));

                if (targetRole == null)
                {
                    var color = GetColorForRank(rank);
                    targetRole = await guild.CreateRoleAsync(targetRoleName,
                        color: color,
                        isMentionable: false,
                        isHoisted: false);
                }

                await user.AddRoleAsync(targetRole);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error assigning Codeforces role");
            }
        }

        private string GetRankEmoji(string rank)
        {
            return rank?.ToLower() switch
            {
                "newbie" => "⚪",
                "pupil" => "🟢",
                "specialist" => "🔵",
                "expert" => "💙",
                "candidate master" => "💜",
                "master" => "🟠",
                "international master" => "🟠",
                "grandmaster" => "🔴",
                "international grandmaster" => "🔴",
                "legendary grandmaster" => "⭐",
                _ => "⚫"
            };
        }

        private string GetRoleNameFromRank(string rank)
        {
            return rank?.ToLower() switch
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
            return rank?.ToLower() switch
            {
                "newbie" => new Color(128, 128, 128),
                "pupil" => new Color(0, 128, 0),
                "specialist" => new Color(3, 168, 158),
                "expert" => new Color(0, 0, 255),
                "candidate master" => new Color(170, 0, 170),
                "master" => new Color(255, 140, 0),
                "international master" => new Color(255, 140, 0),
                "grandmaster" => new Color(255, 0, 0),
                "international grandmaster" => new Color(255, 0, 0),
                "legendary grandmaster" => new Color(170, 0, 0),
                _ => new Color(128, 128, 128)
            };
        }

        private readonly string[] _verificationProblems = new[]
        {
            "4/A", "71/A", "158/A", "50/A", "231/A",
            "282/A", "112/A", "263/A", "96/A", "118/A"
        };

        private string GetRandomProblem()
        {
            var random = new Random();
            return _verificationProblems[random.Next(_verificationProblems.Length)];
        }
    }
}