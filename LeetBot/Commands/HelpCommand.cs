using Discord;
using Discord.WebSocket;
using LeetBot.Interfaces;

namespace LeetBot.Commands
{
    public class HelpCommand : ISlashCommand
    {
        public bool isApiCommand { get; set; } = false;

        public SlashCommandBuilder BuildCommand()
        {
            return new SlashCommandBuilder()
                .WithName("help")
                .WithDescription("Get help and information about available commands");
        }

        public async Task ExecuteAsync(SocketSlashCommand command, ISocketMessageChannel channel)
        {
            var embed = new EmbedBuilder()
                .WithTitle("🤖 LeetBot Help")
                .WithDescription("Welcome to LeetBot! Here are all the available commands:")
                .WithColor(Color.Blue)
                .WithThumbnailUrl("https://leetcode.com/static/images/LeetCode_Sharing.png")
                .WithFooter("LeetBot - Compete in LeetCode challenges with your friends!")
                .WithCurrentTimestamp();

            // Basic Commands
            embed.AddField("🔧 Basic Commands", 
                "`/ping` - Check if the bot is online\n" +
                "`/identify <leetcode_id>` - Verify your LeetCode account\n" +
                "`/help` - Show this help message", 
                inline: false);

            // Challenge Commands
            embed.AddField("⚔️ Challenge Commands", 
                "`/challenge <difficulty> [topic]` - Challenge someone to a LeetCode duel\n" +
                "`/team-challenge` - Create a team-based challenge\n" +
                "`/leave` - Leave your current challenge (if message was deleted)", 
                inline: false);

            // Information Commands
            embed.AddField("📊 Information Commands", 
                "`/leaderboard` - View the server leaderboard", 
                inline: false);

            // Challenge Details
            embed.AddField("🎯 Challenge Details", 
                "**Difficulties:** Easy, Medium, Hard\n" +
                "**Topics:** Array, String, Dynamic Programming, Binary Search, Backtracking, Greedy, Graph, Tree, Linked List, Heap, Hash Table, Recursion, Sorting, Bit Manipulation, Math, Number Theory, Database, Shortest Path, Prefix Sum, Sliding Window, Two Pointers\n" +
                "**Scoring:** Easy=1pt | Medium=2pts | Hard=3pts", 
                inline: false);

            // How to Get Started
            embed.AddField("🚀 Getting Started", 
                "1. Use `/identify <your_leetcode_username>` to verify your account\n" +
                "2. Use `/challenge` to create a 1v1 duel or `/team-challenge` for team battles\n" +
                "3. Click the buttons to join challenges\n" +
                "4. Solve the LeetCode problem and click 'Finish' when done\n" +
                "5. Check `/leaderboard` to see your ranking!", 
                inline: false);

            // Tips
            embed.AddField("💡 Tips", 
                "• Challenges are created in threads for better organization\n" +
                "• You can only be in one challenge at a time\n" +
                "• Use `/leave` if a challenge message gets deleted\n" +
                "• Team challenges allow multiple players per team\n" +
                "• Your LeetCode real name must match the verification code", 
                inline: false);

            await command.RespondAsync(embed: embed.Build(), ephemeral: true);
        }
    }
}
