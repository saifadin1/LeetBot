using Discord;
using Discord.WebSocket;
using LeetBot.DTOs;
using LeetBot.Interfaces;
using LeetBot.Models;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace LeetBot.Services
{
    public class LeetCodeService : ILeetCodeService
    {
        private readonly HttpClient _httpClient;
        private const string url = "https://leetcode.com/graphql"; 
        private readonly ILogger<LeetCodeService> _logger;
        private readonly IChallengeRepo _challengeRepo;

        public LeetCodeService(ILogger<LeetCodeService> logger, HttpClient httpClient, IChallengeRepo challengeRepo)
        {
            _httpClient = httpClient;
            _logger = logger;
            _challengeRepo = challengeRepo;

            _httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/123.0.0.0 Safari/537.36");
            _httpClient.DefaultRequestHeaders.Referrer = new Uri("https://leetcode.com/");
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        }
        public async Task<string> GetUserRealNameAsync(string username)
        {
            var query = new
            {
                operationName = "userPublicProfile",
                query = @"query userPublicProfile($username: String!) {
                            matchedUser(username: $username) {
                                profile {
                                    realName
                                }
                            }
                        }",
                variables = new { username }
            };

            var jsonContent = JsonConvert.SerializeObject(query);
            var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync(url, content);

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception("wrong name");
            }
            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                throw new Exception($"LeetCode API call failed. Status: {response.StatusCode}. Body: {error}");
            }

            Console.WriteLine(response.Content.ToString());
            Console.WriteLine("\n\n\n");

            var responseString = await response.Content.ReadAsStringAsync();
            dynamic data = JsonConvert.DeserializeObject(responseString);

            Console.WriteLine(data.ToString());

            return data.data.matchedUser.profile.realName.ToString();
        }

        public async Task<string> GetRandomProblemAsync(string difficulty, string? topic)
        {
            difficulty = difficulty.ToUpper();

            var query = new
            {
                operationName = "randomQuestionV2",
                query = """
                query randomQuestionV2($favoriteSlug: String, $categorySlug: String, $searchKeyword: String, $filtersV2: QuestionFilterInput) {
                  randomQuestionV2(
                    favoriteSlug: $favoriteSlug
                    categorySlug: $categorySlug
                    filtersV2: $filtersV2
                    searchKeyword: $searchKeyword
                  ) {
                    titleSlug
                  }
                }
                """,
                variables = new
                {
                    favoriteSlug = (string?)null,
                    categorySlug = "all-code-essentials",
                    searchKeyword = "",
                    filtersV2 = new
                    {
                        filterCombineType = "ALL",
                        statusFilter = new
                        {
                            questionStatuses = Array.Empty<string>() 
                        },
                        difficultyFilter = new
                        {
                            difficulties = new[] { difficulty }
                        },
                        premiumFilter = new
                        {
                            premiumStatus = new[] { "NOT_PREMIUM" }
                        },
                        topicFilter = new
                        {
                            topicSlugs = topic is not null ? new[] { topic } : Array.Empty<string>()
                        },
                        companyFilter = new
                        {
                            companySlugs = Array.Empty<string>()
                        },
                        languageFilter = new
                        {
                            languageSlugs = Array.Empty<string>()
                        },
                        positionFilter = new
                        {
                            positionSlugs = Array.Empty<string>()
                        },
                        frequencyFilter = new { },
                        acceptanceFilter = new { },
                        lastSubmittedFilter = new { },
                        publishedFilter = new { }
                    }
                }
            };



            var jsonContent = JsonConvert.SerializeObject(query);

            Console.WriteLine(jsonContent);

            var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");


            var response = await _httpClient.PostAsync(url, content);
            response.EnsureSuccessStatusCode();

            Console.WriteLine("asdasd");


            Console.WriteLine(response.Content.ToString());
            Console.WriteLine("\n\n\n");

            var responseString = await response.Content.ReadAsStringAsync();
            dynamic data = JsonConvert.DeserializeObject(responseString);

            Console.WriteLine(data.ToString());

            string titleSlug = data.data.randomQuestionV2.titleSlug.ToString();
            Console.WriteLine("Extracted titleSlug: " + titleSlug);

            return titleSlug;
            //
            //return "replace-all-s-to-avoid-consecutive-repeating-characters";

        }

        public async Task<UserLastSubmissionDTO> GetUserSubmissionsAsync(string username)
        {
            var query = new
            {
                operationName = "recentAcSubmissions",
                query = @"query recentAcSubmissions($username: String!, $limit: Int!) {
             recentAcSubmissionList(username: $username, limit: $limit) {
                 id
                 title
                 titleSlug
                 timestamp
             }
         }",

                variables = new { username, limit = 1}
            };
            var jsonContent = JsonConvert.SerializeObject(query);
            var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync(url, content);


            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                throw new Exception($"LeetCode API call failed. Status: {response.StatusCode}. Body: {error}");
            }
            var responseString = await response.Content.ReadAsStringAsync();

            _logger.LogInformation("LeetCode response: {Response}", responseString);


            dynamic data = JsonConvert.DeserializeObject(responseString);

            var submissions = data?.data?.recentAcSubmissionList;
            if (submissions == null || submissions.Count == 0)
            {
                _logger.LogWarning("No recent submissions found for user: {Username}", username);
                return null;
            }

            var lastSubmission = new UserLastSubmissionDTO()
            {
                TitleSlug = submissions[0].titleSlug.ToString(),
                TimeStamp = submissions[0].timestamp.ToString(),
                LeetCodeUsername = username
            };

            return lastSubmission;
        }

        //public async Task MonitorChallengeAsync(SocketMessageComponent component, Challenge challenge, SocketThreadChannel threadChannel, CancellationToken cancellationToken)
        //{
        //    var problemSlug = challenge.TitleSlug;
        //    var chanllengerMention = challenge.Challenger.Mention;
        //    var opponentMention = challenge.Opponent.Mention;

        //    var challengerUsername = challenge.Challenger.LeetCodeUsername;
        //    var opponentUsername = challenge.Opponent.LeetCodeUsername;

        //    var endTime = DateTime.UtcNow.AddMinutes(45);
        //    var winner = string.Empty;


        //    try
        //    {
        //        while (DateTime.UtcNow < endTime)
        //        {
        //            cancellationToken.ThrowIfCancellationRequested();
        //            string? challengerLastSubmission = null , opponentLastSubmission = null;

        //            // check if the challenge became empty
        //            if (await _challengeRepo.isEmpty(challenge.Id))
        //            {
        //                //var embed
        //                _logger.LogInformation("Challenge cancelled. both of the users left.");
        //                await threadChannel.DeleteAsync();

        //                return;
        //            }

        //            // Check if the challenge is still active
        //            if (challenge.ChallengerId is not null)
        //                 challengerLastSubmission = await GetUserSubmissionsAsync(challengerUsername);
        //            if (challenge.ChallengerId is not null)
        //                 opponentLastSubmission = await GetUserSubmissionsAsync(opponentUsername);



        //            if (challengerLastSubmission == problemSlug)
        //            {
        //                winner = chanllengerMention;
        //                challenge.Challenger.GamePlayed++;
        //                challenge.Opponent.GamePlayed++;
        //                challenge.Challenger.GameWon++;
        //                switch (challenge.Difficulty) {
        //                    case "easy":
        //                        challenge.Challenger.EasyWon++;
        //                        break;
        //                    case "medium":
        //                        challenge.Challenger.MediumWon++;
        //                        break;
        //                    case "hard":
        //                        challenge.Challenger.HardWon++;
        //                        break;
        //                }
        //                break;
        //            }

        //            if (opponentLastSubmission == problemSlug)
        //            {
        //                winner = opponentMention;
        //                challenge.Challenger.GamePlayed++;
        //                challenge.Opponent.GamePlayed++;
        //                challenge.Opponent.GameWon++;
        //                switch (challenge.Difficulty) {
        //                    case "easy":
        //                        challenge.Opponent.EasyWon++;
        //                        break;
        //                    case "medium":
        //                        challenge.Opponent.MediumWon++;
        //                        break;
        //                    case "hard":
        //                        challenge.Opponent.HardWon++;
        //                        break;
        //                }
        //                break;
        //            }

        //            var random = new Random();
        //            await Task.Delay(random.Next(20000, 25000), cancellationToken);
        //        }


        //        // trying to delete the prev message 
        //        //var message = await channel.GetMessageAsync(component.Message.Id);
        //        //if (message != null)
        //        //{
        //        //    await message.DeleteAsync();
        //        //}


        //        if (string.IsNullOrEmpty(winner))
        //        {

        //            var embed = new EmbedBuilder()
        //                .WithColor(Color.Red)
        //                .WithTitle("Challenge Timed Out")
        //                .WithDescription("No one solved the problem in time!")
        //                .Build();

        //            await component.Message.ModifyAsync(msg =>
        //            {
        //                msg.Embed = embed;
        //                msg.Components = new ComponentBuilder().Build();
        //            });

        //            _logger.LogInformation("Challenge timed out. No one solved the problem in time!");
        //            //await channel.SendMessageAsync(embed: embed);
        //        }
        //        else
        //        {

        //            var embed = new EmbedBuilder()
        //                .WithColor(Color.Green)
        //                .WithTitle("Challenge Ended")
        //                .WithDescription($"{winner} solved the problem first! Congratulations!")
        //                .Build();

        //            await component.Message.ModifyAsync(msg =>
        //            {
        //                msg.Embed = embed;
        //                msg.Components = new ComponentBuilder().Build();
        //            });


        //            _logger.LogInformation($"Challenge ended. Winner: {winner} solved the problem first!");
        //            //await channel.SendMessageAsync(embed: embed);
        //        }

        //        // mention the players to let them know
        //        await threadChannel.SendMessageAsync($"{chanllengerMention} {opponentMention} \n");

        //        //await threadChannel.DeleteAsync();

        //        _challengeRepo.RemoveChallenge(challenge.Id);
        //        await _challengeRepo.SaveChangesAsync();
        //    }
        //    catch (OperationCanceledException)
        //    {
        //        _logger.LogInformation("Challenge monitoring was cancelled.");
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, "Unexpected error in MonitorChallengeAsync.");
        //    }
        //}


        public async Task GetUsersSubmissions(SocketMessageComponent component, Challenge challenge, SocketThreadChannel threadChannel)
        {
            var problemSlug = challenge.TitleSlug;
            var chanllengerMention = challenge.Challenger.Mention;
            var opponentMention = challenge.Opponent.Mention;

            var challengerUsername = challenge.Challenger.LeetCodeUsername;
            var opponentUsername = challenge.Opponent.LeetCodeUsername;

            var winner = string.Empty;


            // logging 
            _logger.LogInformation($"Challenge started between {chanllengerMention} and {opponentMention} for problem: {problemSlug}");




            UserLastSubmissionDTO challengerLastSubmission = null, opponentLastSubmission = null;

            //// check if the challenge became empty
            //if (await _challengeRepo.isEmpty(challenge.Id))
            //{
            //    //var embed
            //    _logger.LogInformation("Challenge cancelled. both of the users left.");
            //    await threadChannel.DeleteAsync();

            //    return;
            //}

            // Check if the challenge is still active
            if (challenge.ChallengerId is not null)
                challengerLastSubmission = await GetUserSubmissionsAsync(challengerUsername);
            if (challenge.OpponentId is not null)
                opponentLastSubmission = await GetUserSubmissionsAsync(opponentUsername);


            bool challengerSolved = challengerLastSubmission is not null
    && challengerLastSubmission.TitleSlug == problemSlug;

            bool opponentSolved = opponentLastSubmission is not null
                && opponentLastSubmission.TitleSlug == problemSlug;

            bool challengerWon = challengerSolved && (
                !opponentSolved || challengerLastSubmission.CompareTo(opponentLastSubmission) < 0
            );

            if (challengerWon)
            {
                winner = chanllengerMention;
                challenge.Challenger.GamePlayed++;
                challenge.Opponent.GamePlayed++;
                challenge.Challenger.GameWon++;

                switch (challenge.Difficulty)
                {
                    case "easy":
                        challenge.Challenger.EasyWon++;
                        break;
                    case "medium":
                        challenge.Challenger.MediumWon++;
                        break;
                    case "hard":
                        challenge.Challenger.HardWon++;
                        break;
                }
            }
            else if (opponentSolved)
            {
                winner = opponentMention;
                challenge.Challenger.GamePlayed++;
                challenge.Opponent.GamePlayed++;
                challenge.Opponent.GameWon++;

                switch (challenge.Difficulty)
                {
                    case "easy":
                        challenge.Opponent.EasyWon++;
                        break;
                    case "medium":
                        challenge.Opponent.MediumWon++;
                        break;
                    case "hard":
                        challenge.Opponent.HardWon++;
                        break;
                }
            }
            else
            {
                // Neither solved
                await component.FollowupAsync("You didn't solve the problem yet, please click only when you've solved it.", ephemeral: true);
                return;
            }



            var embed = new EmbedBuilder()
                    .WithColor(Color.Green)
                    .WithTitle("Challenge Ended")
                    .WithDescription($"{winner} solved the problem first! Congratulations!")
                    .Build();

                await component.Message.ModifyAsync(msg =>
                {
                    msg.Embed = embed;
                    msg.Components = new ComponentBuilder().Build();
                });


                _logger.LogInformation($"Challenge ended. Winner: {winner} solved the problem first!");
                //await channel.SendMessageAsync(embed: embed);
            

                // mention the players to let them know
                await threadChannel.SendMessageAsync($"{chanllengerMention} {opponentMention} \n");

                //await threadChannel.DeleteAsync();

                await _challengeRepo.RemoveChallengeAsync(challenge.Id);
                challenge.Challenger.IsFree = true;
                challenge.Opponent.IsFree = true;
                await _challengeRepo.SaveChangesAsync();
        }
            



        public async Task<List<(int, string)>> GetUserProblemSolved(string username)
        {
            var query = new
            {
                operationName = "userProfileUserQuestionProgressV2",
                query = """
            query userProfileUserQuestionProgressV2($userSlug: String!) {
              userProfileUserQuestionProgressV2(userSlug: $userSlug) {
                numAcceptedQuestions {
                  count
                  difficulty
                }
              }
            }
        """,
                variables = new { userSlug = username }
            };

            var jsonContent = JsonConvert.SerializeObject(query);
            var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Add("Accept", "application/json");

            var response = await _httpClient.PostAsync(url, content);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                throw new Exception($"LeetCode API call failed. Status: {response.StatusCode}. Body: {error}");
            }

            var responseString = await response.Content.ReadAsStringAsync();
            dynamic data = JsonConvert.DeserializeObject(responseString);
            var questions = new List<(int Count, string Difficulty)>();
            foreach (var item in data.data.userProfileUserQuestionProgressV2.numAcceptedQuestions)
            {
                int count = item.count;
                string difficulty = item.difficulty;
                questions.Add((count, difficulty));
            }

            return questions;
        }

        
        public async Task<UserAcceptedQuestionsResponseDTO> GetNumAccQuestionsAsync(string username)
        {
            var query = new
            {
                operationName = "userProfileUserQuestionProgressV2",
                query = """
                    query userProfileUserQuestionProgressV2($userSlug: String!) {
                      userProfileUserQuestionProgressV2(userSlug: $userSlug) {
                        numAcceptedQuestions {
                          count
                          difficulty
                        }
                      }
                    }
                    """,
                variables = new { userSlug = username }
            };

            var jsonContent = JsonConvert.SerializeObject(query);
            var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync("https://leetcode.com/graphql", content);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                throw new Exception($"LeetCode API call failed. Status: {response.StatusCode}. Body: {error}");
            }

            var responseString = await response.Content.ReadAsStringAsync();
            _logger.LogInformation("LeetCode response: {Response}", responseString);

            dynamic data = JsonConvert.DeserializeObject(responseString);
            var acceptedQuestions = data?.data?.userProfileUserQuestionProgressV2?.numAcceptedQuestions;

            if (acceptedQuestions == null)
            {
                _logger.LogWarning("No accepted questions found for user: {Username}", username);
                return null;
            }

            var result = new UserAcceptedQuestionsResponseDTO()
            {
                LeetCodeUsername = username,
                NumAcceptedQuestions = new List<UserAcceptedQuestionsDTO>()
            };

            foreach (var q in acceptedQuestions)
            {
                result.NumAcceptedQuestions.Add(new UserAcceptedQuestionsDTO()
                {
                    Count = q.count,
                    Difficulty = q.difficulty
                });
            }

            return result;
        }

        public async Task<string> GetUserAvatarAsync(string username)
        {
            var query = new
            {
                operationName = "userPublicProfile",
                query = @"query userPublicProfile($username: String!) {
                            matchedUser(username: $username) {
                                profile {
                                    userAvatar
                                }
                            }
                        }",
                variables = new { username }
            };


            var jsonContent = JsonConvert.SerializeObject(query);
            var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync("https://leetcode.com/graphql", content);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                throw new Exception($"LeetCode API call failed. Status: {response.StatusCode}. Body: {error}");
            }

            var responseString = await response.Content.ReadAsStringAsync();
            _logger.LogInformation("LeetCode response: {Response}", responseString);

            dynamic data = JsonConvert.DeserializeObject(responseString);
            var acceptedQuestions = data?.data?.userProfileUserQuestionProgressV2?.numAcceptedQuestions;

            string avatarUrl = data.data.matchedUser.profile.userAvatar.ToString();

            return avatarUrl;
        }

    }
}
