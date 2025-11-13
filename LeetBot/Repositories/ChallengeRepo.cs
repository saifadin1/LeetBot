using Discord;
using Discord.Rest;
using LeetBot.Data;
using LeetBot.Helpers;
using LeetBot.Interfaces;
using LeetBot.Models;
using Microsoft.EntityFrameworkCore;

namespace LeetBot.Repositories
{
    public class ChallengeRepo : IChallengeRepo
    {
        private readonly AppDbContext _dbContext;
        public ChallengeRepo(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }
        public async Task CreateChallengAsync(IDiscordInteraction interaction, RestUserMessage message, string difficulty, string? topic)
        {
            //var message = await interaction.GetOriginalResponseAsync();
            var userId = interaction.User.Id;

            var challenge = new Challenge()
            {
                Id = message.Id,               // Message ID is used as the challenge ID
                ChallengerId = TextProcessor.UserId(userId, interaction.GuildId),
                ChannelId = interaction.ChannelId ?? 0,
                Difficulty = difficulty,
                GuildId = interaction.GuildId,
                Topic = topic,
                StartedAt = DateTime.UtcNow,
                EndedAt = DateTime.UtcNow + TimeSpan.FromMinutes(30)
            };

            await _dbContext.Challenges.AddAsync(challenge);
            await _dbContext.SaveChangesAsync();
        }
        public async Task<Challenge> GetChallengeById(ulong id)
        {
            var challenge = await _dbContext.Challenges
                .Include(c => c.Challenger)
                .Include(c => c.Opponent)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (challenge == null)
                throw new Exception($"Challenge with ID {id} not found.");

            return challenge;
        }

        public async Task<Challenge> GetChallengeByUserId(string userId)
        {
            var challenge = await _dbContext.Challenges
                .Include(c => c.Challenger)
                .Include(c => c.Opponent)
                .FirstOrDefaultAsync(c => c.ChallengerId == userId || c.OpponentId == userId);
            //if (challenge == null)
            //    throw new Exception($"Challenge with User ID {userId} not found.");
            return challenge;
        }

        public async Task<int> SaveChangesAsync()
        {
            return await _dbContext.SaveChangesAsync();
        }

        public void RemoveChallenge(ulong id)
        {
            var challenge = _dbContext.Challenges.Find(id);
            if (challenge != null)
            {
                _dbContext.Challenges.Remove(challenge);
            }
        }

        public async Task<bool> IsUserChallenging(IDiscordInteraction interaction)
        {
            var userId = TextProcessor.UserId(interaction.User.Id, interaction.GuildId);
            var challenge = await _dbContext.Challenges
                .FirstOrDefaultAsync(c => c.ChallengerId == userId || c.OpponentId == userId);
            return challenge != null;
        }

        public async Task<bool> isEmpty(ulong id)
        {
            var challenge = await _dbContext.Challenges
                .FirstOrDefaultAsync(c => c.Id == id);

            if (challenge == null)
            {
                return true;
            }

            return challenge.ChallengerId == null && challenge.OpponentId == null;
        }

        public async Task RemoveChallenger(string userId)
        {
            var isChallenger = _dbContext.Challenges
                .Any(c => c.ChallengerId == userId);


            if (isChallenger)
            {
                var challenge = _dbContext.Challenges.FirstOrDefault(c => c.ChallengerId == userId);
                if (challenge is null)
                {
                    throw new InvalidOperationException("you are not part of any active challenge");
                }
                if (challenge != null)
                {
                    challenge.Challenger = null;
                    challenge.ChallengerId = null;
                }

                return;
            }
            var isOpponent = _dbContext.Challenges
                .Any(c => c.OpponentId == userId);
            if (isOpponent)
            {
                var challenge = _dbContext.Challenges.FirstOrDefault(c => c.OpponentId == userId);
                if (challenge != null)
                {
                    challenge.Opponent = null;
                    challenge.OpponentId = null;
                }

                return;
            }


            throw new InvalidOperationException("you are not part of any active challenge");
        }
    }
}
