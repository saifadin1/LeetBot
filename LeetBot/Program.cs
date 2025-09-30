using LeetBot.Commands;
using LeetBot.ComponentHandlers.Challenge;
using LeetBot.ComponentHandlers.TeamChallenge;
using LeetBot.ComponentHandlers.TeamChallenge.Joins;
using LeetBot.Data;
using LeetBot.Interfaces;
using LeetBot.Repositories;
using LeetBot.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Hosting;
using System.Reactive.Linq;
using System.Reflection;

namespace LeetBot
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            var host = Host.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration((hostingContext, config) =>
                {
                    config.AddJsonFile(Path.Combine(AppContext.BaseDirectory, "appsettings.json"),
                        optional: true, reloadOnChange: true);
                    config.AddEnvironmentVariables();

                })
                .ConfigureLogging((context, logging) =>
                {
                    logging.ClearProviders();
                    logging.AddConsole();
                    logging.SetMinimumLevel(LogLevel.Error);
                })
                .ConfigureServices((context, services) =>
                {
                    // config & http client
                    services.AddHttpClient();
                    services.AddSingleton<IConfiguration>(context.Configuration);

                    // EF Core
                    services.AddDbContext<AppDbContext>(options =>
                        options.UseNpgsql(context.Configuration.GetConnectionString("DefaultConnection")));

                    // your domain services
                    services.AddScoped<IBot, Bot>();

                    services.AddScoped<IdentifyCommand>();
                    services.AddScoped<ChallengeCommand>();
                    services.AddScoped<TeamChallengeCommand>();
                    services.AddScoped<PingCommand>();
                    services.AddScoped<LeaderboardCommand>();
                    services.AddScoped<LeaveFromChallangCommand>();
                    services.AddScoped<HelpCommand>();

                    services.AddScoped<JoinBtnHandler>();
                    services.AddScoped<LeaveBtnHandler>();
                    services.AddScoped<FinishBtnHandler>();
                    services.AddScoped<JoinTeam1BtnHandler>();
                    services.AddScoped<JoinTeam2BtnHandler>();
                    services.AddScoped<StartTeamBtnHandler>();
                    services.AddScoped<leaveTeamBtnHandler>();

                    services.AddScoped<ILeetCodeService, LeetCodeService>();
                    services.AddScoped<ITeamService, TeamService>();

                    services.AddScoped<IChallengeRepo, ChallengeRepo>();
                    services.AddScoped<IUserRepo, UserRepo>();
                    services.AddScoped<ITeamRepo, TeamRepo>();
                    services.AddScoped<ITeamChallengeRepo, TeamChallengeRepo>();

                    // Hosted service that starts/stops the bot
                    services.AddHostedService<BotHostedService>();
                })
                .UseConsoleLifetime()
                .Build();


            await host.RunAsync();
        }
    }
}
