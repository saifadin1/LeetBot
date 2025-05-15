using Discord.WebSocket;
using LeetBot.Interfaces;
using LeetBot.Models;
using LeetBot.Repositories;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LeetBot.ComponentHandlers.Challenge
{
    internal class FinishBtnHandler : IComponentHandler
    {
        private readonly ILogger<FinishBtnHandler> _logger;
        private readonly ILeetCodeService _leetCodeService;
        private readonly IChallengeRepo _challengeRepo;

        public FinishBtnHandler(ILeetCodeService leetCodeService, IChallengeRepo challenegRepo, ILogger<FinishBtnHandler> logger)
        {
            _leetCodeService = leetCodeService;
            _challengeRepo = challenegRepo;
            _logger = logger;
        }
        public string CustomId => "finish_btn";

        public async Task ExecuteAsync(SocketMessageComponent component, SocketThreadChannel threadChannel)
        {
            await component.DeferAsync(ephemeral: true);
            var userId = $"{component.User.Id}-{component.GuildId}";
            var challenge = await _challengeRepo.GetChallengeByUserId(userId);
            _logger.LogInformation($"Challenge: {challenge.TitleSlug}");
            await _leetCodeService.GetUsersSubmissions(component, challenge, threadChannel);
        }
    }
}
