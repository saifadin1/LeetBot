using Discord;
using Discord.WebSocket;
using LeetBot.Data;
using LeetBot.Interfaces;
using LeetBot.Models;
using LeetBot.Repositories;
using Microsoft.EntityFrameworkCore;
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

        private async Task WaitForBotReady(CancellationToken stoppingToken)
        {
            while (_client.ConnectionState != ConnectionState.Connected )
            {
                if (stoppingToken.IsCancellationRequested) return;
                _logger.LogInformation("Polling service is waiting for bot to be ready...");
                await Task.Delay(1000, stoppingToken);
            }
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await WaitForBotReady(stoppingToken);
            if (stoppingToken.IsCancellationRequested) return;


            _logger.LogInformation("Challenge Polling Service is starting.");
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
                        var teamRepo = scope.ServiceProvider.GetRequiredService<ITeamRepo>();
                        var userRepo = scope.ServiceProvider.GetRequiredService<IUserRepo>();
                        var soloChallengeService = scope.ServiceProvider.GetRequiredService<ISoloChallengeService>();

                        var expiredSoloChallenges = await challengeRepo.GetExpiredChallengesAsync();
                        var expiredTeamChallenges = await teamChallengeRepo.GetExpiredChallengesAsync();




                        foreach (var challenge in expiredSoloChallenges)
                        {


                            // unlock challenger and opponent
                            if (challenge.ChallengerId != null)
                            {
                                await userRepo.UnlockUserAsync(challenge.ChallengerId);
                            }
                            if (challenge.OpponentId != null)
                            {
                                await userRepo.UnlockUserAsync(challenge.OpponentId);
                            }



                            await ProcessExpiredChallenge(
                               challenge.Id,
                               challenge.ChannelId,
                               async () =>
                               {
                                   return await soloChallengeService.BuildTeamChallengeResultEmbedAsync(challenge.Id);
                               },
                               challenge
                           );


                            await challengeRepo.RemoveChallengeAsync(challenge.Id);
                        }



                        foreach (var teamChallenge in expiredTeamChallenges)
                        {
                            var teamsToDisband = await teamRepo.GetTeamsByChallengeIdAsync(teamChallenge.Id);

                            foreach (var team in teamsToDisband)
                            {
                                foreach (var user in team.Users)
                                {
                                    user.TeamId = null;
                                    user.IsFree = true;
                                }

                                _logger.LogInformation("Disbanded Team {TeamId} and freed {UserCount} users.", team.Id, team.Users.Count);
                            }
                            await ProcessExpiredChallenge(
                               teamChallenge.Id,
                               teamChallenge.ChannelId,
                               async () =>
                               {
                                   return await teamService.BuildTeamChallengeResultEmbedAsync(teamChallenge.Id);
                               },
                               teamChallenge
                           );


                            await teamChallengeRepo.DeleteTeamChallengeAsync(teamChallenge.Id);
                        }


                        if (expiredSoloChallenges.Any() || expiredTeamChallenges.Any())
                        {
                            await dbContext.SaveChangesAsync(stoppingToken);
                            _logger.LogInformation("Processed {Count} expired challenges.", expiredSoloChallenges.Count() + expiredTeamChallenges.Count());
                        }
                    }
                } 
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred in Challenge Polling Service.");
                }

                await Task.Delay(TimeSpan.FromSeconds(60), stoppingToken);
            }

            _logger.LogInformation("Challeneg polling service is stopping..");
        }

        public async Task ProcessExpiredChallenge(
            ulong challengeId ,
            ulong channelId , 
            Func<Task<Embed>> buildResultEmbedAsync,
            BaseChallenge challengeEntity)
        {
            var channel = _client.GetChannel(channelId) as ISocketMessageChannel;
            if (channel == null)
            {
                _logger.LogWarning("Could not find channel {ChannelId} for expired challenge {ChallengeId}",
                    channelId, challengeId);
                challengeEntity.IsActive = false;
                return;
            }

            
            IMessage originalMessage;
            try
            {
                originalMessage = await channel.GetMessageAsync(challengeId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get message {MessageId} in channel {ChannelId}", challengeId, channelId);
                originalMessage = null;
            }


            if (originalMessage == null || !(originalMessage is IUserMessage userMessage))
            {
                _logger.LogWarning("Could not find original message {MessageId} for challenge {ChallengeId}",
                    challengeId, challengeId);
                
                await channel.SendMessageAsync($"Time's up! The challenge (ID: {challengeId}) is now closed. (Original message not found)");
                challengeEntity.IsActive = false;
                return;
            }

            try
            {
                var finalEmbed = await buildResultEmbedAsync();

                await userMessage.ModifyAsync(props =>
                {
                    props.Embed = finalEmbed;
                    props.Components = new ComponentBuilder().Build();
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to build results or modify message for challenge {ChallengeId}", challengeId);
                await channel.SendMessageAsync($"Time's up! Challenge {challengeId} is closed. An error occurred building the results.");
            }

            challengeEntity.IsActive = false;
        }
    }
}
