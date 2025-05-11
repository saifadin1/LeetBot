using Discord;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LeetBot.Models
{
    public class TeamChallenge
    {
        public long Id { get; set; }
        public long? GuildId { get; set; }
        public DateTime StartedAt { get; set; }
        public DateTime EndedAt { get; set; }
        public ICollection<Team> Teams { get; set; } = new List<Team>();

    }
}
