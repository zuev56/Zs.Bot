﻿using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using Zs.Bot.Helpers;
using Zs.Bot.Messenger.Telegram;
using Zs.Bot.Model.Db;
using Zs.Bot.Modules.Messaging;
using Zs.Common.Abstractions;
using Zs.Common.Extensions;
using Zs.Common.Modules.Connectors;
using Zs.Service.Home.Model.Db;

namespace Zs.Service.Home.Bot
{
    class Program
    {
        private static int _reloadCounter = 0;


        public static async Task Main(string[] args)
        {
            try
            {
                if (args?.Length == 0)
                {
                    var localConfig = Path.Combine(Directory.GetCurrentDirectory(), "configuration.json");
                    args = new[] { localConfig };
                }

                if (args?.Length != 1)
                    throw new ArgumentException("Wrong number of arguments");

                if (!File.Exists(args[0]))
                    throw new FileNotFoundException($"Wrong configuration path:\n{args[0]}");

                await ServiceLoader(args[0]);
            }
            catch (Exception ex)
            {
                _reloadCounter++;
                Console.WriteLine($"\n\n{ex}\nMessage:"
                                + $"\n{ex.Message}"
                                + $"\n\nStackTrace:\n{ex.StackTrace}");
                Console.ReadKey();
            }
        }

        public static async Task ServiceLoader(string configPath)
        {
            try
            {
                if (configPath is null)
                    throw new ArgumentNullException(nameof(configPath));

                var builder = new HostBuilder()
                    .ConfigureAppConfiguration((hostingContext, config) =>
                    {
                        config.AddJsonFile(
                            configPath,
                            optional: false, // is required
                            reloadOnChange: true);
                    })
                    .ConfigureServices((hostContext, services) =>
                    {
                        services.AddDbContext<ZsBotDbContext>(options  =>
                            options.UseNpgsql(hostContext.Configuration.GetConnectionString("Home"))
                                   .EnableDetailedErrors(true)
                                   .EnableSensitiveDataLogging(true));

                        services.AddDbContext<HomeDbContext>(options =>
                            options.UseNpgsql(hostContext.Configuration.GetConnectionString("Home"))
                                   .EnableDetailedErrors(true)
                                   .EnableSensitiveDataLogging(true));

                        services.AddTransient<IConnectionAnalyser, ConnectionAnalyser>(sp =>
                        {
                            var ca = new ConnectionAnalyser(Logger.GetInstance(), "https://vk.com/", "https://yandex.ru/", "https://www.google.ru/");
                            if (hostContext.Configuration["ProxySocket"] != null)
                                ca.InitializeProxy(
                                    hostContext.Configuration["ProxySocket"],
                                    hostContext.Configuration["ProxyLogin"],
                                    hostContext.Configuration["ProxyPassword"]
                                    );
                            return ca;
                        });

                        services.AddTransient<IMessenger, TelegramMessenger>(sp =>
                            new TelegramMessenger(hostContext.Configuration["BotToken"], sp.GetService<IConnectionAnalyser>().WebProxy));
                        
                        services.AddSingleton<IHostedService, UserWatcher>(x =>
                            ActivatorUtilities.CreateInstance<UserWatcher>(x));
                    });

                await builder.RunConsoleAsync();
            }
            catch (Exception ex)
            {
                _reloadCounter++;
                //Console.WriteLine(JsonSerializer.Serialize(ex).NormalizeJsonString());
                Console.WriteLine(JsonConvert.SerializeObject(ex).NormalizeJsonString());

                if (_reloadCounter < 3)
                {
                    Thread.Sleep(1000);
                    await ServiceLoader(configPath).ConfigureAwait(false);
                }

                Console.ReadLine();
            }
        }
    }
}
