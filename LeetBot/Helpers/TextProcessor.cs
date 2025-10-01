using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LeetBot.Helpers
{
    public static class TextProcessor
    {
        public static string ProblemLink(string titleSlug)
        {
            return $"https://leetcode.com/problems/{titleSlug}/";
        }

        public static string UserId(ulong userId, ulong? guildId)
        {
            return $"{userId}-{guildId}";
        }
    }
}
