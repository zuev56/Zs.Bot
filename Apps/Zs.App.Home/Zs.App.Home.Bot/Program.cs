﻿using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using Zs.Bot.Messenger.Telegram;
using Zs.Bot.Model.Data;
using Zs.Bot.Services.Logging;
using Zs.Bot.Services.Messaging;
using Zs.Common.Abstractions;
using Zs.Common.Extensions;
using Zs.Common.Modules.Connectors;
using Zs.App.Home.Model.Data;
using Zs.App.Home.Model;
using BotContextFactory = Zs.Bot.Model.Factories.BotContextFactory;
using HomeContextFactory = Zs.App.Home.Model.ContextFactory;
using Zs.App.Home.Model.Abstractions;
using Zs.Bot;

namespace Zs.App.Home.Bot
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
                        services.AddDbContext<HomeContext>(options =>
                            options.UseNpgsql(hostContext.Configuration.GetConnectionString("Default"))
                                   .EnableDetailedErrors(true)
                                   .EnableSensitiveDataLogging(true));

                        services.AddDbContext<BotContext>(options  =>
                            options.UseNpgsql(hostContext.Configuration.GetConnectionString("Default"))
                                   .EnableDetailedErrors(true)
                                   .EnableSensitiveDataLogging(true));

                        services.AddSingleton<IContextFactory, HomeContextFactory>(sp =>
                            new HomeContextFactory(sp.GetService<DbContextOptions<BotContext>>(), sp.GetService<DbContextOptions<HomeContext>>()));

                        services.AddSingleton<IContextFactory<BotContext>, BotContextFactory>(sp =>
                            new BotContextFactory(sp.GetService<DbContextOptions<BotContext>>()));

                        services.AddSingleton<IZsLogger, Logger>(sp => new Logger(sp.GetService<IContextFactory<BotContext>>()));

                        services.AddScoped<IConnectionAnalyser, ConnectionAnalyser>(sp =>
                        {
                            var ca = new ConnectionAnalyser(sp.GetService<IZsLogger>(), "https://vk.com/", "https://yandex.ru/", "https://www.google.ru/");
                            if (hostContext.Configuration["ProxySocket"] != null)
                                ca.InitializeProxy(
                                    hostContext.Configuration["ProxySocket"],
                                    hostContext.Configuration["ProxyLogin"],
                                    hostContext.Configuration["ProxyPassword"]
                                    );
                            return ca;
                        });

                        services.AddSingleton<IMessenger, TelegramMessenger>(sp =>
                            new TelegramMessenger(hostContext.Configuration["BotToken"],
                                sp.GetService<IContextFactory<BotContext>>(),
                                sp.GetService<IZsLogger>(),
                                sp.GetService<IConnectionAnalyser>().WebProxy)
                            );

                        services.AddSingleton<IZsBot, ZsBot>(sp =>
                            new ZsBot(
                                sp.GetService<IConfiguration>(),
                                sp.GetService<IMessenger>(),
                                sp.GetService<IContextFactory<BotContext>>(),
                                sp.GetService<IZsLogger>())
                            );

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
