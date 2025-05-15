using Discord.WebSocket;
using LeetBot.Helpers;
using LeetBot.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LeetBot.ComponentHandlers.TeamChallenge
{
    public class leaveTeamBtnHandler : IComponentHandler
    {
        public string CustomId { get; set; } = "leaveTeamBtn";
        private readonly IUserRepo _userRepo;
        private readonly ITeamRepo _teamRepo;
        public leaveTeamBtnHandler(IUserRepo userRepo, ITeamRepo teamRepo)
        {
            _userRepo = userRepo;
            _teamRepo = teamRepo;
        }
        public async Task ExecuteAsync(SocketMessageComponent component, SocketThreadChannel threadChannel)
        {
            await component.DeferAsync();

            var user = await _userRepo.GetUserByIdAsync(TextProcessor.UserId(component.User.Id, component.GuildId));
            await _teamRepo.RemoveUserFromTeamAsync(user.TeamId, user);
            await _userRepo.UnlockUserAsync(component);
            await component.FollowupAsync("You have left the team", ephemeral: true);
        }
    }
}
