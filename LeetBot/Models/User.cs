using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LeetBot.Models
{
    public class User
    {
        public string Id { get; set; }
        public string Username { get; set; } = null!;
        public string? LeetCodeUsername { get; set; }
        public long? GuildId { get; set; }
        public string Mention { get; set; } = null!;
        public DateTime VerifiedAt { get; set; } = DateTime.UtcNow;
        public int GamePlayed { get; set; } = 0;
        public int GameWon { get; set; } = 0;
        public int EasyWon { get; set; } = 0;
        public int MediumWon { get; set; } = 0;
        public int HardWon { get; set; } = 0;

        public double WinRatio => GamePlayed == 0 ? 0 : Math.Round((double)GameWon / GamePlayed, 2);
        public double WinPercentage => GamePlayed == 0 ? 0 : Math.Round(((double)GameWon / GamePlayed) * 100, 1);

        public int DifficultyScore => EasyWon + MediumWon * 2 + HardWon * 3;



        // team stuff
        public long? TeamId { get; set; }
        public Team? Team { get; set; }


    }
}
