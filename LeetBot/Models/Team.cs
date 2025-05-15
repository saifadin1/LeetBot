using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LeetBot.Models
{
    public class Team
    {
        public long Id { get; set; }

        public long ChallengeId { get; set; }
        public TeamChallenge TeamChallenge { get; set; }

        public int Score { get; set; } = 0;
        public ICollection<User> Users { get; set; } = new List<User>();
    }
}
