using System;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace What_A_Bot.Worker
{
    public class WhatABotWorker : BackgroundService
    {
        private static ILogger<WhatABotWorker> _logger;
        private readonly IConfiguration _configuration;
        private DiscordSocketClient _client;

        public WhatABotWorker(ILogger<WhatABotWorker> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                _client = new DiscordSocketClient();
                _client.Log += LogAsync;
                _client.MessageReceived += ClientMessageReceived;

                var token = _configuration["DiscordToken"] ?? throw new Exception("Discord token was not supplied.");

                await _client.LoginAsync(TokenType.Bot, token);
                await _client.StartAsync();
                await Task.Delay(-1, stoppingToken);
            }
        }

        private static async Task ClientMessageReceived(SocketMessage message)
        {
            const char emptyChar = ' ';

            if (!message.Content.StartsWith("!") || message.Author.IsBot)
            {
                await Task.CompletedTask;
            }

            var lengthOfCommand = message.Content.Contains(emptyChar)
                ? message.Content.IndexOf(emptyChar)
                : message.Content.Length;

            var command = message.Content.Substring(1, lengthOfCommand - 1);

            await LogAsync(new LogMessage(LogSeverity.Info, "Code",
                $"Message with command of {message.Content} by {message.Author} received at {message.Channel}."));

            if (command.Equals("HelloBot", StringComparison.CurrentCultureIgnoreCase))
            {
                var text =
                    $"Hello {message.Author}. I am What-A-Bot.{Environment.NewLine}{Environment.NewLine}I only exist right now as a worker process on a Web Service, however this will change. Plans will include processing and cleansing of commands and logic to be run through Azure functions so commands may be added via JavaScript and C#!.{Environment.NewLine}{Environment.NewLine}I will reside here: https://github.com/adamasmar/What-A-Bot.";
                await message.Channel.SendMessageAsync(text);
                await LogAsync(new LogMessage(LogSeverity.Info, "Code",
                    $"Message sent to {message.Channel}. Content = {text}"));
            }//
        }

        private static async Task LogAsync(LogMessage logMessage)
        {
            await Task.Run(() =>
            {
                var exceptionText = string.IsNullOrWhiteSpace(logMessage.Exception?.StackTrace)
                    ? string.Empty
                    : $" (StackTrace = {logMessage.Exception?.StackTrace}";
                var messageText = $"{logMessage.Message}{exceptionText}";

                switch (logMessage.Severity)
                {
                    case LogSeverity.Critical:
                        _logger.LogCritical(messageText);
                        break;
                    case LogSeverity.Error:
                        _logger.LogError(messageText);
                        break;
                    case LogSeverity.Warning:
                        _logger.LogWarning(messageText);
                        break;
                    case LogSeverity.Info:
                        _logger.LogInformation(messageText);
                        break;
                    case LogSeverity.Verbose:
                        _logger.LogTrace(messageText);
                        break;
                    case LogSeverity.Debug:
                        _logger.LogDebug(messageText);
                        break;
                    default:
                        _logger.LogInformation(messageText);
                        break;
                }
            });
        }
    }
}
