using Discord;
using Discord.WebSocket;
using LeetBot.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LeetBot.ComponentHandlers
{
    internal class LeaveBtnHandler : IComponentHandler
    {
        private readonly IChallengeRepo _challengeRepo;

        public LeaveBtnHandler(IChallengeRepo challengeRepo)
        {
            _challengeRepo = challengeRepo;
        }

        public string CustomId => "leave_btn";

        public async Task ExecuteAsync(SocketMessageComponent component, SocketThreadChannel threadChannel)
        {
            var interaction = (SocketInteraction)component;

            var userId = $"{component.User.Id}-{component.GuildId}";
            var challenge = await _challengeRepo.GetChallengeByUserId(userId);

            if (challenge == null)                      
            {
                await component.RespondAsync("Challenge not found.", ephemeral: true);
                return;
            }

            try
            {
                await _challengeRepo.RemoveChallenger(userId);
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

                    await threadChannel.DeleteAsync();
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



// TODO: 
// leave command 

