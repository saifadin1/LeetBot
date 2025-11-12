using Discord.WebSocket;
using LeetBot.DTOs; 
using LeetBot.Models;

namespace LeetBot.Interfaces
{
    public interface ILeetCodeService
    {
        Task<string> GetUserRealNameAsync(string username);
        Task<string> GetRandomProblemAsync(string difficulty, string? topic);
        Task<UserLastSubmissionDTO> GetUserSubmissionsAsync(string username);
        Task<List<(int, string)>> GetUserProblemSolved(string username);
        Task GetUsersSubmissions(SocketMessageComponent component, Challenge challenge, SocketThreadChannel threadChannel);
        Task<UserAcceptedQuestionsResponseDTO> GetNumAccQuestionsAsync(string username);
        
        //Task MonitorChallengeAsync(SocketMessageComponent component, Challenge challenge, SocketThreadChannel threadChannel, CancellationToken cancellationToken);
    }
}


// get the last sub for every user in the challenge 
// and compare the submission time then update the scores according to diff btn