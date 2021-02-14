using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Zs.App.Home.Data;
using Zs.App.Home.Data.Models.Vk;
using Zs.App.Home.Services.Vk;
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
using Zs.Common.Services.Abstractions;
using Zs.Common.Services.Connection;
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
                var text = $"\n\n{ex}\nMessage:\n{ex.Message}\n\nStackTrace:\n{ex.StackTrace}";

                TrySaveFailInfo(text);
                Console.WriteLine(text);

                Console.ReadKey();
            }
        }

        private static void TrySaveFailInfo(string text)
        {
            try
            {
                string path = System.IO.Path.Combine(Directory.GetCurrentDirectory(), $"Critical_failure_{DateTime.Now::yyyy.MM.dd HH:mm:ss.ff}.log");
                File.AppendAllText(path, text);
            }
            catch { }
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

                        services.AddSingleton<IContextFactory<BotContext>, BotContextFactory>();                        
                        services.AddSingleton<IContextFactory<HomeContext>, HomeContextFactory>();
                        services.AddSingleton<IZsLogger, Logger>();

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

                        services.AddScoped<IMessageDataSaver, MessageDataDBSaver>();

                        services.AddScoped<IMessenger, TelegramMessenger>(sp =>
                            new TelegramMessenger(
                                hostContext.Configuration["BotToken"],
                                sp.GetService<IItemsWithRawDataRepository<Chat, int>>(),
                                sp.GetService<IItemsWithRawDataRepository<Zs.Bot.Data.Models.User, int>>(),
                                sp.GetService<IItemsWithRawDataRepository<Message, int>>(),
                                sp.GetService<IMessageDataSaver>(),
                                sp.GetService<ICommandManager>(),
                                sp.GetService<IConnectionAnalyser>().WebProxy,
                                sp.GetService<IZsLogger>())
                            );

                        services.AddScoped<IRepository<ActivityLogItem, int>, CommonRepository<HomeContext, ActivityLogItem, int>>();
                        services.AddScoped<IRepository<Data.Models.Vk.User, int>, CommonRepository<HomeContext, Data.Models.Vk.User, int>>();

                        services.AddScoped<IRepository<Log, int>, CommonRepository<BotContext, Log, int>>();
                        services.AddScoped<IItemsWithRawDataRepository<Chat, int>, ItemsWithRawDataRepository<BotContext, Chat, int>>();
                        services.AddScoped<IItemsWithRawDataRepository<Zs.Bot.Data.Models.User, int>, ItemsWithRawDataRepository<BotContext, Zs.Bot.Data.Models.User, int>>();
                        services.AddScoped<IItemsWithRawDataRepository<Message, int>, ItemsWithRawDataRepository<BotContext, Message, int>>();

                        services.AddScoped<IActivityService, ActivityService>();

                        services.AddScoped<IScheduler, Scheduler>();

                        services.AddScoped<IRepository<Command, string>, CommonRepository<BotContext, Command, string>>();
                        services.AddScoped<IRepository<UserRole, string>, CommonRepository<BotContext, UserRole, string>>();
                        services.AddScoped<ICommandManager, CommandManager>(sp =>
                            new CommandManager(
                                hostContext.Configuration.GetConnectionString("Default"),
                                sp.GetService<IRepository<Command, string>>(),
                                sp.GetService<IRepository<UserRole, string>>(),
                                sp.GetService<IItemsWithRawDataRepository<Zs.Bot.Data.Models.User, int>>(),
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
