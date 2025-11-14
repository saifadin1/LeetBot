namespace LeetBot.Models
{
    public class TeamChallenge : BaseChallenge
    {
        public ulong Id { get; set; }
        public ulong? GuildId { get; set; }
        public ulong ChannelId { get; set; }
        public DateTime StartedAt { get; set; }
        public DateTime EndedAt { get; set; }
        public ICollection<Team> Teams { get; set; } = new List<Team>();
        public string? EasyProblemTitleSlug { get; set; }
        public string? MediumProblemTitleSlug { get; set; }
        public string? HardProblemTitleSlug { get; set; }
        public int Team1CurrentScore { get; set; } = 0;
        public int Team2CurrentScore { get; set; } = 0;
        public int Team1MaxPossibleScore { get; set; } = 600;
        public int Team2MaxPossibleScore { get; set; } = 600;
        public int Problem1SolvedByTeam { get; set; } = 0;
        public int Problem2SolvedByTeam { get; set; } = 0;
        public int Problem3SolvedByTeam { get; set; } = 0;

    }
}
