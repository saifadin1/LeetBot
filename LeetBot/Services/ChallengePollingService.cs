using Discord.WebSocket;
using LeetBot.Data;
using LeetBot.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LeetBot.Services
{
    internal class ChallengePollingService : BackgroundService
    {
        private readonly ILogger<ChallengePollingService> _logger;
        private readonly DiscordSocketClient _client;
        private readonly IServiceProvider _serviceProvider;

        public ChallengePollingService(
            ILogger<ChallengePollingService> logger,
            DiscordSocketClient client,
            IServiceProvider serviceProvider)
        {
            _logger = logger;
            _client = client;
            _serviceProvider = serviceProvider;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using (var scope = _serviceProvider.CreateScope())
                    {
                        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                        var challengeRepo = scope.ServiceProvider.GetRequiredService<IChallengeRepo>();
                        var teamChallengeRepo = scope.ServiceProvider.GetRequiredService<ITeamChallengeRepo>();
                        var teamService = scope.ServiceProvider.GetRequiredService<ITeamService>();
                        var leetCodeService = scope.ServiceProvider.GetRequiredService<ILeetCodeService>();

                    }
                } 
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred in Challenge Polling Service.");
                }

                await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
            }

            _logger.LogInformation("Challeneg polling service is stopping..");
        }
    }
}
