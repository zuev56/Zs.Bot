using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Zs.App.Home.Model;
using Zs.App.Home.Model.Data;
using Zs.Bot.Data;
using Zs.Bot.Data.Abstractions;
using Zs.Bot.Data.Models;
using Zs.Bot.Data.Repositories;
using Zs.Bot.Messenger.Telegram;
using Zs.Bot.Services.Commands;
using Zs.Bot.Services.DataSavers;
using Zs.Bot.Services.Logging;
using Zs.Bot.Services.Messaging;
using Zs.Common.Abstractions;
using Zs.Common.Extensions;
using Zs.Common.Services.Connectors;
using Zs.Common.Services.Scheduler;
using BotContextFactory = Zs.Bot.Data.Factories.BotContextFactory;

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
                    var localConfig = System.IO.Path.Combine(Directory.GetCurrentDirectory(), "configuration.json");
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

                        services.AddSingleton<IContextFactory<BotContext>, BotContextFactory>(sp =>
                            new BotContextFactory(sp.GetService<DbContextOptions<BotContext>>()));
                        
                        services.AddSingleton<IContextFactory<HomeContext>, HomeContextFactory>(sp =>
                            new HomeContextFactory(sp.GetService<DbContextOptions<HomeContext>>()));

                        services.AddSingleton<IZsLogger, Logger>(sp => new Logger(sp.GetService<IRepository<Log, int>>()));

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

                        services.AddScoped<IMessageDataSaver, MessageDataDBSaver>(sp =>
                            new MessageDataDBSaver(
                                sp.GetService<IItemsWithRawDataRepository<Chat, int>>(),
                                sp.GetService<IItemsWithRawDataRepository<User, int>>(),
                                sp.GetService<IItemsWithRawDataRepository<Message, int>>(),
                                sp.GetService<IZsLogger>())
                            );

                        services.AddScoped<IMessenger, TelegramMessenger>(sp =>
                            new TelegramMessenger(
                                hostContext.Configuration["BotToken"],
                                sp.GetService<IItemsWithRawDataRepository<Chat, int>>(),
                                sp.GetService<IItemsWithRawDataRepository<User, int>>(),
                                sp.GetService<IItemsWithRawDataRepository<Message, int>>(),
                                sp.GetService<IMessageDataSaver>(),
                                sp.GetService<ICommandManager>(),
                                sp.GetService<IConnectionAnalyser>().WebProxy,
                                sp.GetService<IZsLogger>())
                            );

                        services.AddScoped<IRepository<VkActivityLogItem, int>, CommonRepository<HomeContext, VkActivityLogItem, int>>(sp =>
                            new CommonRepository<HomeContext, VkActivityLogItem, int>(
                                sp.GetService<IContextFactory<HomeContext>>())
                            );
                        services.AddScoped<IRepository<VkUser, int>, CommonRepository<HomeContext, VkUser, int>>(sp =>
                            new CommonRepository<HomeContext, VkUser, int>(
                                sp.GetService<IContextFactory<HomeContext>>())
                            );

                        services.AddScoped<IRepository<Log, int>, CommonRepository<BotContext, Log, int>>(sp =>
                            new CommonRepository<BotContext, Log, int>(
                                sp.GetService<IContextFactory<BotContext>>())
                            );
                        services.AddScoped<IItemsWithRawDataRepository<Chat, int>, ItemsWithRawDataRepository<BotContext, Chat, int>>(sp =>
                            new ItemsWithRawDataRepository<BotContext, Chat, int>(
                                sp.GetService<IContextFactory<BotContext>>())
                            );
                        services.AddScoped<IItemsWithRawDataRepository<User, int>, ItemsWithRawDataRepository<BotContext, User, int>>(sp =>
                            new ItemsWithRawDataRepository<BotContext, User, int>(
                                sp.GetService<IContextFactory<BotContext>>())
                            );
                        services.AddScoped<IItemsWithRawDataRepository<Message, int>, ItemsWithRawDataRepository<BotContext, Message, int>>(sp =>
                            new ItemsWithRawDataRepository<BotContext, Message, int>(
                                sp.GetService<IContextFactory<BotContext>>())
                            );

                        services.AddScoped<IScheduler, Scheduler>(sp =>
                            new Scheduler(hostContext.Configuration, sp.GetService<IZsLogger>())
                            );

                        services.AddScoped<IRepository<Command, string>, CommonRepository<BotContext, Command, string>>(sp =>
                            new CommonRepository<BotContext, Command, string>(
                                sp.GetService<IContextFactory<BotContext>>())
                            );
                        services.AddScoped<IRepository<UserRole, string>, CommonRepository<BotContext, UserRole, string>>(sp =>
                            new CommonRepository<BotContext, UserRole, string>(
                                sp.GetService<IContextFactory<BotContext>>())
                            );
                        services.AddScoped<ICommandManager, CommandManager>(sp =>
                            new CommandManager(
                                hostContext.Configuration.GetConnectionString("Default"),
                                sp.GetService<IRepository<Command, string>>(),
                                sp.GetService<IRepository<UserRole, string>>(),
                                sp.GetService<IItemsWithRawDataRepository<User, int>>(),
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
