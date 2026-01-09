using LeetBot.Interfaces;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace LeetBot.Services
{
    public class CodeforcesService : ICodeforcesService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<CodeforcesService> _logger;
        private const string API_BASE = "https://codeforces.com/api";

        public CodeforcesService(HttpClient httpClient, ILogger<CodeforcesService> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public async Task<CodeforcesUserInfo> GetUserInfoAsync(string handle)
        {
            try
            {
                var response = await _httpClient.GetAsync($"{API_BASE}/user.info?handles={handle}");

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("CF API Error for {Handle}: {Code}", handle, response.StatusCode);
                    return null;
                }

                var content = await response.Content.ReadAsStringAsync();
                var jsonDoc = JsonDocument.Parse(content);

                if (jsonDoc.RootElement.GetProperty("status").GetString() != "OK")
                {
                    _logger.LogWarning("CF API Status NOT OK for {Handle}", handle);
                    return null;
                }

                var userArray = jsonDoc.RootElement.GetProperty("result");
                if (userArray.GetArrayLength() == 0)
                    return null;

                var user = userArray[0];

                return new CodeforcesUserInfo
                {
                    Handle = user.GetProperty("handle").GetString(),
                    FirstName = user.TryGetProperty("firstName", out var fn) ? fn.GetString() : "",
                    LastName = user.TryGetProperty("lastName", out var ln) ? ln.GetString() : "",
                    Rating = user.TryGetProperty("rating", out var r) ? r.GetInt32() : 0,
                    Rank = user.TryGetProperty("rank", out var rank) ? rank.GetString() : "unrated",
                    MaxRating = user.TryGetProperty("maxRating", out var mr) ? mr.GetInt32() : 0,
                    MaxRank = user.TryGetProperty("maxRank", out var maxRank) ? maxRank.GetString() : "unrated",
                    Avatar = user.TryGetProperty("avatar", out var av) ? av.GetString() : ""

                };
            }
            catch (JsonException)
            {
                _logger.LogError("Failed to parse JSON from Codeforces (likely downtime/Cloudflare).");
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error fetching user info.");
                return null;
            }
        }

        public async Task<List<CodeforcesSubmission>> GetRecentSubmissionsAsync(string handle, int count)
        {
            try
            {
                var response = await _httpClient.GetAsync(
                    $"{API_BASE}/user.status?handle={handle}&from=1&count={count}");



                if (!response.IsSuccessStatusCode) return new List<CodeforcesSubmission>();

                var content = await response.Content.ReadAsStringAsync();
                var jsonDoc = JsonDocument.Parse(content);

                if (jsonDoc.RootElement.GetProperty("status").GetString() != "OK") 
                    return new List<CodeforcesSubmission>();

                var submissions = new List<CodeforcesSubmission>();
                var submissionsArray = jsonDoc.RootElement.GetProperty("result");

                foreach (var sub in submissionsArray.EnumerateArray())
                {
                    var problem = sub.GetProperty("problem");

                    submissions.Add(new CodeforcesSubmission
                    {
                        Id = sub.GetProperty("id").GetInt32(),
                        ContestId = problem.TryGetProperty("contestId", out var cid)
                            ? cid.GetInt32().ToString() : "0",
                        ProblemIndex = problem.GetProperty("index").GetString(),
                        Verdict = sub.TryGetProperty("verdict", out var v) ? v.GetString() : "TESTING",
                        ProgrammingLanguage = sub.TryGetProperty("programmingLanguage", out var pl)
                            ? pl.GetString() : "",
                        SourceCode = "", // Source code not available via API without auth
                        CreationTimeSeconds = sub.GetProperty("creationTimeSeconds").GetInt64()
                    });
                }

                return submissions;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching submissions from Codeforces for handle: {Handle}", handle);
                return new List<CodeforcesSubmission>();
            }
        }


        public async Task<string> GetUserFirstNameAsync(string handle)
        {
            var userInfo = await GetUserInfoAsync(handle);
            return userInfo.FirstName ?? "";
        }
    }
}