using Discord;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LeetBot.Interfaces
{
    internal interface ISoloChallengeService
    {
        Task<Embed> BuildTeamChallengeResultEmbedAsync(ulong id);
    }
}
