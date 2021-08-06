using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace What_A_Bot.Server.Services
{
    public class DiscordBotWorker : BackgroundService
    {
        private readonly ILogger<DiscordBotWorker> _logger;
        private readonly IConfiguration _configuration;

        public DiscordBotWorker(ILogger<DiscordBotWorker> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation(
                    $"Discord bot worker is running at {DateTime.UtcNow}. Token is {_configuration["DiscordToken"]}");
                await Task.Delay(1000, stoppingToken);
            }
        }
    }
}
