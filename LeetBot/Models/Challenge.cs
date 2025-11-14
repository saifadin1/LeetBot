using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LeetBot.Models
{
    public class Challenge : BaseChallenge
    {
        public ulong Id { get; set; }
        public string? TitleSlug { get; set; }
        public string? Difficulty { get; set; }
        public string? Topic { get; set; }   // this might be List in future 
        public string? ProblemLink { get; set; }
        public string? ChallengerId { get; set; }
        public User? Challenger { get; set; }
        public string? OpponentId { get; set; }
        public User? Opponent { get; set; }
        public ulong? GuildId { get; set; }
        public ulong ChannelId { get; set; }
        public DateTime StartedAt { get; set; }
        public DateTime EndedAt { get; set; }




    }
}
