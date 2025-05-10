using Discord.Commands;
using Discord.WebSocket;
using Discord;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;
using LeetBot.Interfaces;
using Microsoft.Extensions.Logging;

namespace LeetBot
{
    internal class Bot : IBot
    {
        ILogger<Bot> _logger;
        private ServiceProvider? _serviceProvider;

        private readonly IConfiguration _configuration;
        private readonly DiscordSocketClient _client;
        private readonly CommandService _commands;

        private readonly Dictionary<string, ISlashCommand> _commandHandlers = new();
        private readonly Dictionary<string, IComponentHandler> _componentHandlers = new();

        private static SocketTextChannel channel { get; set; } = null!;



        public Bot(IConfiguration configuration, ILogger<Bot> logger)
        {
            _configuration = configuration;
            _logger = logger;

            DiscordSocketConfig config = new DiscordSocketConfig()
            {
                GatewayIntents = GatewayIntents.AllUnprivileged | GatewayIntents.MessageContent | GatewayIntents.Guilds | GatewayIntents.GuildMessages,
                LogLevel = LogSeverity.Debug,
                AlwaysDownloadUsers = true,
                DefaultRetryMode = RetryMode.AlwaysRetry
            };


            _client = new DiscordSocketClient(config);
            _commands = new CommandService();


        }

        public async Task StartAsync(ServiceProvider services)
        {
            string discordToken = _configuration["Discord:BotToken"] ?? throw new Exception("Missing Discord token");

            _serviceProvider = services;

            await _commands.AddModulesAsync(Assembly.GetExecutingAssembly(), _serviceProvider);



            _client.Ready += Client_Ready;
            _client.SlashCommandExecuted += SlashCommandHandler;
            _client.ButtonExecuted += ComponentHandler;
            _client.SelectMenuExecuted += ComponentHandler;


            

            await _client.LoginAsync(TokenType.Bot, discordToken);
            await _client.StartAsync();
        }

        public async Task Client_Ready()
        {
            var assembly = Assembly.GetExecutingAssembly();

            var slashCommandTypes = assembly
                .GetTypes()
                .Where(t => typeof(ISlashCommand).IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract);

            foreach (var type in slashCommandTypes)
            {
                if (_serviceProvider.GetService(type) is ISlashCommand commandInstance)
                {
                    var command = commandInstance.BuildCommand();
                    _commandHandlers[command.Name] = commandInstance;

                    try
                    {
                        await _client.CreateGlobalApplicationCommandAsync(command.Build());
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Failed to create command '{command.Name}': {ex.Message}");
                    }
                }
            }

            var componentHandlerTypes = assembly
                .GetTypes()
                .Where(t => typeof(IComponentHandler).IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract);

            foreach (var type in componentHandlerTypes)
            {
                if (_serviceProvider.GetService(type) is IComponentHandler handler)
                {
                    _componentHandlers[handler.CustomId] = handler;
                }
            }

            Console.WriteLine($"Bot status: {_client.ConnectionState}");

            _logger.LogInformation($"Guilde:{_client.Guilds.Count}");

            

            channel = await _client.GetChannelAsync(1363517565116092613) as SocketTextChannel;

            _logger.LogInformation("Channel: {Channel}", channel);

            _logger.LogInformation("Bot is ready and commands are registered.");
        }

        private async Task SlashCommandHandler(SocketSlashCommand command)
        {
            try
            {
                // make sure the command is in a guild
                if (command.GuildId == null)
                {
                    await command.RespondAsync("This command can only be used in a server.", ephemeral: true);
                    return;
                }

                if (_commandHandlers.TryGetValue(command.Data.Name, out var handler))
                {
                    if (handler.isApiCommand)
                    {
                        var timeToWait = RateLimiter.isRateLimited(command);
                        if (timeToWait.TotalSeconds > 0)
                        {
                            var embed = new EmbedBuilder()
                                .WithTitle("Rate Limit")
                                .WithDescription($"You are being rate limited. Please wait {TimeSpan.FromSeconds(30).TotalSeconds - timeToWait.Seconds} seconds.")
                                .WithColor(Color.Red);

                            await command.RespondAsync(embed: embed.Build(), ephemeral:true);
                            return;
                        }
                    }
                    await handler.ExecuteAsync(command, command.Channel);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error executing command: {CommandName}", command.Data.Name);
                await command.FollowupAsync("An error occurred while processing your command.", ephemeral:true);
            }
        }
        private async Task ComponentHandler(SocketMessageComponent component)
        {
            try
            {
                //if (component.Channel is null)
                //{
                //    _logger.LogError("Channel is null");
                //    return;
                //}
                //var channelId = component.Channel.Id;

                //var channel = await _client.GetChannelAsync(channelId) as SocketTextChannel;



                var thread = component.Channel as SocketThreadChannel;

                if (_componentHandlers.TryGetValue(component.Data.CustomId, out var handler))
                {
                    await handler.ExecuteAsync(component, thread);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error executing component: {ComponentId}", component.Data.CustomId);
                await component.RespondAsync("An error occurred while processing your component.", ephemeral:true);
            }
        }

        public async Task StopAsync()
        {
            if (_client != null)
            {
                await _client.LogoutAsync();
                await _client.StopAsync();
            }
        }
    }
}
