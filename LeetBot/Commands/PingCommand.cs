using Discord;
using Discord.WebSocket;
using LeetBot.Interfaces;

namespace LeetBot.Commands
{
    public class PingCommand : ISlashCommand
    {
        public bool isApiCommand { get; set; } = false;

        public SlashCommandBuilder BuildCommand()
        {
            return new SlashCommandBuilder()
                .WithName("ping")
                .WithDescription("Ping the bot");
        }

        public async Task ExecuteAsync(SocketSlashCommand command, ISocketMessageChannel channel)
        {
            await command.DeferAsync();

            await command.FollowupAsync("pong!");
        }
    }
}
