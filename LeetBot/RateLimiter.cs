using Discord.WebSocket;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LeetBot
{
    internal static class RateLimiter
    {
        private static readonly ConcurrentDictionary<(ulong 
            
            
            
            , string CommandName), DateTime> _requests = new();

        public static TimeSpan isRateLimited(SocketSlashCommand command)
        {
            var userId = command.User.Id;
            var commandName = command.CommandName;
            var key = (userId, commandName);
            var now = DateTime.UtcNow;
            if (_requests.TryGetValue(key, out var lastRequest))
            {
                if ((now - lastRequest).TotalSeconds < 30)
                {
                    return (now - lastRequest);
                }
            }
            _requests[key] = now;
            return TimeSpan.Zero; 
        }
    }
}
