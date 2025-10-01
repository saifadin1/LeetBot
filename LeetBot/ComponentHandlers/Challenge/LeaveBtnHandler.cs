using Discord;
using Discord.WebSocket;
using LeetBot.Helpers;
using LeetBot.Interfaces;
using LeetBot.Repositories;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace LeetBot.ComponentHandlers.Challenge
{
    internal class LeaveBtnHandler : IComponentHandler
    {
        private readonly IChallengeRepo _challengeRepo;
        private readonly IUserRepo _userRepo;

        public LeaveBtnHandler(IChallengeRepo challengeRepo, IUserRepo userRepo)
        {
            _challengeRepo = challengeRepo;
            _userRepo = userRepo;
        }

        public string CustomId => "leave_btn";

        public async Task ExecuteAsync(SocketMessageComponent component, SocketThreadChannel threadChannel)
        {
            var interaction = (SocketInteraction)component;

            var userId = TextProcessor.UserId(component.User.Id, component.GuildId);
            var challenge = await _challengeRepo.GetChallengeByUserId(userId);

            if (challenge == null)                      
            {
                await component.RespondAsync("Challenge not found.", ephemeral: true);
                return;
            }

            try
            {
                await _challengeRepo.RemoveChallenger(userId);
                await _userRepo.UnlockUserAsync(component);
                await _challengeRepo.SaveChangesAsync();

                // check if both players left the challenge
                var isEmpty = await _challengeRepo.isEmpty(challenge.Id);
                if (isEmpty)
                {
                    _challengeRepo.RemoveChallenge(challenge.Id);
                    await _challengeRepo.SaveChangesAsync();



                    await component.DeferAsync();

                    var updatedEmbed = new EmbedBuilder()
                        .WithColor(Color.DarkRed)
                        .WithTitle("Challenge Ended")
                        .WithDescription("Both players have left the challenge. The game has been cancelled.")
                        .Build();

                    await component.Message.ModifyAsync(msg =>
                    {
                        msg.Embed = updatedEmbed;
                        msg.Components = new ComponentBuilder().Build();
                    });

                    //await threadChannel.DeleteAsync();
                }
                else
                {
                    await component.RespondAsync($"{component.User.Username} has left the challenge.");
                }
            }
            catch (Exception ex)
            {
                await component.RespondAsync(ex.Message);
                return;
            }
        }
    }
}

