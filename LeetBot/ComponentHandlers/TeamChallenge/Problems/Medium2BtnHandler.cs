using Discord;
using Discord.WebSocket;
using LeetBot.Interfaces;

namespace LeetBot.ComponentHandlers.TeamChallenge.Problems
{
    public class Medium2BtnHandler : IComponentHandler
    {
        private readonly ITeamService _teamService;

        public Medium2BtnHandler(ITeamService teamService)
        {
            _teamService = teamService;
        }

        public string CustomId => "teamMedium2";

        public async Task ExecuteAsync(SocketMessageComponent component, SocketThreadChannel threadChannel)
        {
            await component.DeferAsync();
            await _teamService.HandleDifficultyButton(component, threadChannel, "medium2");
        }
    }
}