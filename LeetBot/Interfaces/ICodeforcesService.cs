namespace LeetBot.Interfaces
{
    public interface ICodeforcesService
    {
        Task<CodeforcesUserInfo> GetUserInfoAsync(string handle);
        Task<string> GetUserFirstNameAsync(string handle);
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
    }
}