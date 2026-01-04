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

        // Problem slugs
        public string? EasyProblemTitleSlug { get; set; }
        public string? MediumProblem1TitleSlug { get; set; }
        public string? MediumProblem2TitleSlug { get; set; }
        public string? HardProblemTitleSlug { get; set; }

        // Scores
        public int Team1CurrentScore { get; set; } = 0;
        public int Team2CurrentScore { get; set; } = 0;
        public int Team1MaxPossibleScore { get; set; } = 900; // Updated to 900
        public int Team2MaxPossibleScore { get; set; } = 900; // Updated to 900

        // Problem solved tracking (0 = unsolved, 1 = team1, 2 = team2)
        public int Problem1SolvedByTeam { get; set; } = 0; // Easy
        public int Problem2SolvedByTeam { get; set; } = 0; // Medium 1
        public int Problem3SolvedByTeam { get; set; } = 0; // Medium 2
        public int Problem4SolvedByTeam { get; set; } = 0; // Hard
    }
}