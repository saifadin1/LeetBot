using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LeetBot.DTOs
{
    internal class UserLastSubmissionDTO : IComparable
    {
        public string? TitleSlug { get; set; }
        public string? TimeStamp { get; set; }

        public int CompareTo(object? obj)
        {
            if (obj is not UserLastSubmissionDTO other) return 0;

            var thisTime = DateTimeOffset.FromUnixTimeSeconds(long.Parse(TimeStamp!)).UtcDateTime;
            var otherTime = DateTimeOffset.FromUnixTimeSeconds(long.Parse(other.TimeStamp!)).UtcDateTime;

            return DateTime.Compare(thisTime, otherTime);
        }
    }
}
