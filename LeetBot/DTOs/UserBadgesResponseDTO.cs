using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LeetBot.DTOs
{
    public class UserBadgesResponseDTO
    {
        public string LeetCodeUsername { get; set; }
        public List<BadgeDTO> Badges { get; set; } = new List<BadgeDTO>();
    }
}
