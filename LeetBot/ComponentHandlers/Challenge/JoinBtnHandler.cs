using Discord;
using Discord.WebSocket;
using LeetBot.Interfaces;
using Microsoft.Extensions.Logging;

namespace LeetBot.ComponentHandlers.Challenge
{
    internal class JoinBtnHandler : IComponentHandler
    {
        ILogger<JoinBtnHandler> _logger;
        private readonly ILeetCodeService _leetCodeService;
        private readonly IChallengeRepo _challengeRepo;
        private readonly IUserRepo _userRepo;
        public string CustomId => "join_btn";
        public JoinBtnHandler(
            ILeetCodeService leetCodeService,
            IChallengeRepo challengeRepo,
            IUserRepo userRepo,
            ILogger<JoinBtnHandler> logger)
        {
            _leetCodeService = leetCodeService;
            _challengeRepo = challengeRepo;
            _userRepo = userRepo;
            _logger = logger;
        }

        public async Task ExecuteAsync(SocketMessageComponent component, SocketThreadChannel threadChannel)
        {
            if (threadChannel is null)
            {
                _logger.LogError("Channel is null");
                return;
            }

            var interaction = (IComponentInteraction)component;
            var challengeId = interaction.Message.Id;

            // check if this user verifed
            var existingUser = await _userRepo.IsUserExistAsync(component);

            if (existingUser == false)
            {
                await component.RespondAsync("You need to verify yourself first using the /identify command.");
                return;
            }

            // check if user is already in challenge 
            if (!await _userRepo.IsUserFreeAsync(component))
            {
                await component.RespondAsync("You are already in a challenge.", ephemeral: true);
                return;
            }

            await _userRepo.LockUserAsync(component);


            var challenge = await _challengeRepo.GetChallengeById(challengeId);

            if (challenge == null || challenge.ChallengerId == null)
            {
                await component.RespondAsync("Challenge not found.", ephemeral: true);
                return;
            }

            var difficulty = challenge.Difficulty;
            var topic = challenge.Topic;

            // get a random problem from leetcode tages with difficulty
            var slug = await _leetCodeService.GetRandomProblemAsync(difficulty, topic);


            challenge.OpponentId = $"{component.User.Id}-{component.GuildId}";
            challenge.TitleSlug = slug;
            challenge.StartedAt = DateTime.UtcNow;

            await _challengeRepo.SaveChangesAsync();

            var embed = new EmbedBuilder()
                .WithColor(Color.Blue)
                .WithTitle($"Challenge Accepted!")
                .WithDescription($"⚔️ {component.User.Mention} has accepted the challenge! \n\n" +
                $"**Problem:** [LeetCode Problem](https://leetcode.com/problems/{slug}) \n" +
                $"**Difficulty:** {difficulty} \n" +
                $"**Topic:** {(topic is not null ? topic : "Random")} \n" +
                $"**Challenger:** {challenge.Challenger.Mention} \n" +
                $"**Opponent:** {component.User.Mention} \n"
                );

            var components = new ComponentBuilder()
                .WithButton($"leave", "leave_btn", ButtonStyle.Danger)
                .WithButton($"finished🏁", "finish_btn", ButtonStyle.Success);
                
            await component.DeferAsync();

            await component.Message.ModifyAsync(msg =>
            {
                msg.Embed = embed.Build();
                msg.Components = components.Build();
            });



            // remove the prev message
            //await interaction.Message.DeleteAsync();

            // save the current message id 

            // listen for two competitors accounts to determine the winner 

            //try
            //{
            //    var cts = new CancellationTokenSource();
            //    _ = Task.Run(() => _leetCodeService.MonitorChallengeAsync(component, challenge, threadChannel, cts.Token));
            //}
            //catch (Exception ex)
            //{
            //    _logger.LogError(ex, "Error while monitoring challenge");
            //    await component.FollowupAsync($"{ex.Message}", ephemeral: true);
            //    return;
            //}
        }
    }
}
