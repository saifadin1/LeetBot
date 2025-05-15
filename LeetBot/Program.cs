using LeetBot.Commands;
using LeetBot.ComponentHandlers.Challenge;
using LeetBot.Data;
using LeetBot.Interfaces;
using LeetBot.Repositories;
using LeetBot.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Reactive.Linq;
using System.Reflection;

namespace LeetBot
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            var configuration = new ConfigurationBuilder()
                .AddJsonFile(Path.Combine(AppContext.BaseDirectory, "appsettings.json"), optional: true, reloadOnChange: true)
                .AddEnvironmentVariables()
                .Build();

            var serviceProvider = new ServiceCollection()
                .AddLogging(builder =>
                {
                    builder.AddConsole();
                    builder.SetMinimumLevel(LogLevel.Error);
                })
                .AddHttpClient()
                .AddDbContext<AppDbContext>(options => {
                    options.UseNpgsql(configuration.GetConnectionString("DefaultConnection"));
                })
                .AddSingleton<IConfiguration>(configuration)
                .AddScoped<IBot, Bot>()
                .AddScoped<IdentifyCommand>()
                .AddScoped<ChallengeCommand>()
                .AddScoped<TeamChallengeCommand>()
                .AddScoped<PingCommand>()
                .AddScoped<LeaderboardCommand>()
                .AddScoped<LeaveFromChallangCommand>()
                .AddScoped<JoinBtnHandler>()
                .AddScoped<LeaveBtnHandler>()
                .AddScoped<FinishBtnHandler>()
                .AddScoped<ILeetCodeService, LeetCodeService>()
                .AddScoped<ITeamService, TeamService>()
                .AddScoped<IChallengeRepo, ChallengeRepo>()
                .AddScoped<IUserRepo, UserRepo>()
                .AddScoped<ITeamRepo, TeamRepo>()
                .AddScoped<ITeamChallengeRepo, TeamChallengeRepo>()
                .BuildServiceProvider();

                

            try
            {
                IBot bot = serviceProvider.GetRequiredService<IBot>();

                try
                {
                    await bot.StartAsync(serviceProvider);
                }
                catch (Exception exception)
                {
                    Console.WriteLine(exception.Message);
                    Environment.Exit(-1);
                }

                Console.WriteLine("Connected to Discord");

                //dbContext = serviceProvider.GetRequiredService<AppDbContext>();
                //var allChallenges = await dbContext.Challenges.ExecuteDeleteAsync();

                do
                { 
                    await Task.Delay(1000);

                } while (true);
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception.Message);
                Environment.Exit(-1);
            }
        }
    }
}
