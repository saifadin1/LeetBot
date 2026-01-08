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
                response.EnsureSuccessStatusCode();

                var content = await response.Content.ReadAsStringAsync();
                var jsonDoc = JsonDocument.Parse(content);

                if (jsonDoc.RootElement.GetProperty("status").GetString() != "OK")
                {
                    throw new Exception("Codeforces API returned non-OK status");
                }

                var userArray = jsonDoc.RootElement.GetProperty("result");
                if (userArray.GetArrayLength() == 0)
                {
                    throw new Exception("User not found");
                }

                var user = userArray[0];

                return new CodeforcesUserInfo
                {
                    Handle = user.GetProperty("handle").GetString(),
                    FirstName = user.TryGetProperty("firstName", out var fn) ? fn.GetString() : "",
                    LastName = user.TryGetProperty("lastName", out var ln) ? ln.GetString() : "",
                    Rating = user.TryGetProperty("rating", out var r) ? r.GetInt32() : 0,
                    Rank = user.TryGetProperty("rank", out var rank) ? rank.GetString() : "unrated",
                    MaxRating = user.TryGetProperty("maxRating", out var mr) ? mr.GetInt32() : 0,
                    MaxRank = user.TryGetProperty("maxRank", out var maxRank) ? maxRank.GetString() : "unrated"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching user info from Codeforces for handle: {Handle}", handle);
                throw;
            }
        }

        public async Task<string> GetUserFirstNameAsync(string handle)
        {
            var userInfo = await GetUserInfoAsync(handle);
            return userInfo.FirstName ?? "";
        }
    }
}