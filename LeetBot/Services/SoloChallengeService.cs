using Discord;
using LeetBot.Interfaces;
using LeetBot.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LeetBot.Services
{
    internal class SoloChallengeService : ISoloChallengeService
    {
        private readonly IChallengeRepo _challengeRepo;
        public SoloChallengeService(IChallengeRepo challengeRepo)
        {
            _challengeRepo = challengeRepo;
        }

        public async Task<Embed> BuildTeamChallengeResultEmbedAsync(ulong id)
        {
            var challenge = await _challengeRepo.GetChallengeById(id);

            if (challenge == null)
            {
                return new EmbedBuilder()
                    .WithTitle("Challenge Not Found")
                    .WithDescription($"Could not calculate results for challenge {id} as it was not found.")
                    .WithColor(Color.Red)
                    .Build();
            }

            // 2. Determine result (Draw by timeout)
            var title = "Time's Up! It's a Draw! ⌛";
            var color = Color.LightGrey;
            var description = "The challenge timer has expired, and no winner was declared.";

            var embedBuilder = new EmbedBuilder()
                .WithTitle(title)
                .WithDescription(description)
                .WithColor(color)
                .WithTimestamp(DateTimeOffset.UtcNow)
                .WithFooter("Challenge complete");

            // 3. Add problem info
            embedBuilder.AddField("Problem", challenge.TitleSlug ?? "Not Set", inline: true);
            embedBuilder.AddField("Difficulty", challenge.Difficulty ?? "N/A", inline: true);
            embedBuilder.AddField("Topic", challenge.Topic ?? "N/A", inline: true);

            // 4. Get player info
            // This requires your GetChallengeByIdAsync to .Include(c => c.Challenger) and .Include(c => c.Opponent)
            var challengerName = challenge.Challenger?.Mention ?? "Challenger";
            var opponentName = challenge.Opponent?.Mention ?? "Opponent";

            embedBuilder.AddField("\u200B", "\u200B"); // Blank field for spacing
            embedBuilder.AddField("Challenger", challengerName, inline: true);
            embedBuilder.AddField("\u200B", "\u200B", inline: true); // Spacer
            embedBuilder.AddField("Opponent", opponentName, inline: true);

            return embedBuilder.Build();
        }
    }
}
