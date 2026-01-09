namespace LeetBot.Interfaces
{
    public interface ICodeforcesService
    {
        Task<CodeforcesUserInfo> GetUserInfoAsync(string handle);
        Task<string> GetUserFirstNameAsync(string handle);
        Task<List<CodeforcesSubmission>> GetRecentSubmissionsAsync(string handle, int count);
    }

    public class CodeforcesUserInfo
    {
        public string Handle { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public int Rating { get; set; }
        public string Rank { get; set; }
        public int MaxRating { get; set; }
        public string MaxRank { get; set; }
        public string Avatar { get; set; }  
    }

    public class CodeforcesSubmission
    {
        public int Id { get; set; }
        public string ContestId { get; set; }
        public string ProblemIndex { get; set; }
        public string Verdict { get; set; }
        public string ProgrammingLanguage { get; set; }
        public string SourceCode { get; set; }
        public long CreationTimeSeconds { get; set; }
    }
}