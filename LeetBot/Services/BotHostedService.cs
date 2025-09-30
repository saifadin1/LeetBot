using LeetBot.Data;
using LeetBot.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LeetBot.Services
{
    internal class BotHostedService : IHostedService
    {
        private readonly IBot _bot;
        private readonly IServiceProvider _serviceProvider;

        public BotHostedService(IBot bot, IServiceProvider serviceProvider)
        {
            _bot = bot;
            _serviceProvider = serviceProvider;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            // Reset all users' IsFree to false
            using (var scope = _serviceProvider.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                await dbContext.Users
                    .Where(u => u.IsFree == false)
                    .ExecuteUpdateAsync(s => s.SetProperty(u => u.IsFree, true), cancellationToken);
            }

            await _bot.StartAsync(_serviceProvider);
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            if (_bot is Bot concreteBot)
            {
                await concreteBot.StopAsync();
            }
        }
    }
}
