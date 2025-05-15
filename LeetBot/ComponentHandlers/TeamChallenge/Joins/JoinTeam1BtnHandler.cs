using Discord;
using Discord.WebSocket;
using LeetBot.Interfaces;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LeetBot.ComponentHandlers.TeamChallenge.Joins
{
    public class JoinTeam1BtnHandler : IComponentHandler
    {
        private readonly ITeamChallengeRepo _teamChallengeRepo;
        private readonly IUserRepo _userRepo;
        private readonly ITeamRepo _teamRepo;
        private readonly ILogger<JoinTeam1BtnHandler> _logger;
        private readonly ITeamService _teamService;


        public string CustomId => "joinTeam1Btn";

        public JoinTeam1BtnHandler(ITeamChallengeRepo teamChallengeRepo,
            IUserRepo userRepo, 
            ILogger<JoinTeam1BtnHandler> logger, 
            ITeamRepo teamRepo,
            ITeamService teamService)
        {
            _teamChallengeRepo = teamChallengeRepo;
            _userRepo = userRepo;
            _logger = logger;
            _teamRepo = teamRepo;
            _teamService = teamService;
        }

        public async Task ExecuteAsync(SocketMessageComponent component, SocketThreadChannel threadChannel)
        {
            await _teamService.HandleJoinTeamAsync(component, 1);
        }
    }
}
