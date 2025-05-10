using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LeetBot.Interfaces
{
    internal interface IBot
    {
        Task StartAsync(ServiceProvider services);
        Task StopAsync();
    }
}
