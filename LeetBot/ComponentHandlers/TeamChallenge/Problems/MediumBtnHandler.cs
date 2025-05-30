﻿using Discord;
using Discord.WebSocket;
using LeetBot.DTOs;
using LeetBot.Helpers;
using LeetBot.Interfaces;
using LeetBot.Models;
using LeetBot.Repositories;
using Microsoft.Extensions.Logging;

namespace LeetBot.ComponentHandlers.TeamChallenge.Problems
{
    public class MediumBtnHandler : IComponentHandler
    {
        private readonly ITeamService _teamService;


        public MediumBtnHandler(ITeamService teamService)
        {
            _teamService = teamService;
        }

        public string CustomId => "teamMedium";

        public async Task ExecuteAsync(SocketMessageComponent component, SocketThreadChannel threadChannel)
        {
            await _teamService.HandleDifficultyButton(component, threadChannel, "medium");

        }
    }
}
