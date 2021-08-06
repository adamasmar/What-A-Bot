using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using Azure.Identity;
using Microsoft.Extensions.DependencyInjection;
using What_A_Bot.Server.Services;
using What_A_Bot.Worker;

namespace What_A_Bot.Server
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration((context, config) =>
                {
                    var configuration = config.Build();

                    if (context.HostingEnvironment.IsProduction())
                    {
                        var azAppConfigConnection = configuration["AppConfig"];

                        if (!string.IsNullOrEmpty(azAppConfigConnection))
                        {
                            config.AddAzureAppConfiguration(options =>
                            {
                                options.Connect(azAppConfigConnection)
                                    .ConfigureRefresh(refresh =>
                                    {
                                        refresh.Register("TestApp:Settings:Sentinel", true);
                                    });
                            });
                        }
                        else
                        {
                            if (Uri.TryCreate(configuration["Endpoints:AppConfig"], UriKind.Absolute, out var endpoint))
                            {
                                config.AddAzureAppConfiguration(options =>
                                {
                                    options.Connect(endpoint, new DefaultAzureCredential())
                                        .ConfigureRefresh(refresh =>
                                        {
                                            refresh.Register("TestApp:Settings:Sentinel", true);
                                        });
                                });
                            }
                        }
                    }
                    else
                    {
                        config.AddUserSecrets<Program>();
                    }
                })
                .ConfigureLogging(logging =>
                {
                    logging.ClearProviders();
                    logging.AddConsole();
                })
                .ConfigureServices(services =>
                {
                    services.AddHostedService<WhatABotWorker>();
                    services.AddControllersWithViews();

                })
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
    }
}
