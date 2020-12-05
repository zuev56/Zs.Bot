using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Zs.App.ChatAdmin.Abstractions;
using Zs.App.ChatAdmin.Data;
using Zs.Bot.Data;
using Zs.Bot.Data.Abstractions;
using Zs.Bot.Data.Factories;
using Zs.Bot.Data.Models;
using Zs.Bot.Messenger.Telegram;
using Zs.Bot.Services.Commands;
using Zs.Bot.Services.DataSavers;
using Zs.Bot.Services.Logging;
using Zs.Bot.Services.Messaging;
using Zs.Common.Abstractions;
using Zs.Common.Services.Connectors;
using Zs.Common.Services.Scheduler;
using ChatAdminContextFactory = Zs.App.ChatAdmin.Data.ContextFactory;

namespace Zs.App.ChatAdmin
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

                await ServiceLoader(args[0]).ConfigureAwait(false);
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
                var builder = new HostBuilder()
                    .ConfigureAppConfiguration((hostingContext, config) =>
                    {
                        config.AddJsonFile(configPath, optional: false, reloadOnChange: true);
                    })
                    .ConfigureServices((hostContext, services) =>
                    {
                        services.AddDbContext<ChatAdminContext>(options =>
                            options.UseNpgsql(hostContext.Configuration.GetConnectionString("Default"))
                                   .EnableDetailedErrors(true)
                                   .EnableSensitiveDataLogging(true));

                        services.AddDbContext<BotContext>(options =>
                            options.UseNpgsql(hostContext.Configuration.GetConnectionString("Default"))
                                   .EnableDetailedErrors(true)
                                   .EnableSensitiveDataLogging(true));

                        services.AddSingleton<IContextFactory, ChatAdminContextFactory>(sp =>
                            new ChatAdminContextFactory(sp.GetService<DbContextOptions<BotContext>>(), sp.GetService<DbContextOptions<ChatAdminContext>>()));

                        services.AddSingleton<IContextFactory<BotContext>, BotContextFactory>(sp =>
                            new BotContextFactory(sp.GetService<DbContextOptions<BotContext>>()));

                        services.AddSingleton<IZsLogger, Logger>(sp => new Logger(sp.GetService<IContextFactory<BotContext>>()));

                        services.AddScoped<IConnectionAnalyser, ConnectionAnalyser>(sp =>
                        {
                            var ca = new ConnectionAnalyser(sp.GetService<IZsLogger>(),
                                "https://vk.com/", "https://yandex.ru/", "https://www.google.ru/");
                            if (hostContext.Configuration["Proxy:Socket"] != null)
                                ca.InitializeProxy(hostContext.Configuration["Proxy:Socket"],
                                    hostContext.Configuration["Proxy:Login"],
                                    hostContext.Configuration["Proxy:Password"]);
                            return ca;
                        });

                        services.AddScoped<IMessenger, TelegramMessenger>(sp => 
                            new TelegramMessenger(
                                hostContext.Configuration["BotToken"],
                                sp.GetService<IItemsWithRawDataRepository<Chat, int>>(),
                                sp.GetService<IItemsWithRawDataRepository<User, int>>(),
                                sp.GetService<IItemsWithRawDataRepository<Message, int>>(),
                                sp.GetService<IMessageDataSaver>(),
                                sp.GetService<ICommandManager>(),
                                sp.GetService<IZsLogger>(),
                                sp.GetService<IConnectionAnalyser>().WebProxy)
                            );

                        services.AddScoped<IMessageDataSaver, MessageDataDBSaver>(sp => 
                            new MessageDataDBSaver(
                                sp.GetService<IItemsWithRawDataRepository<Chat, int>>(),
                                sp.GetService<IItemsWithRawDataRepository<User, int>>(),
                                sp.GetService<IItemsWithRawDataRepository<Message, int>>(),
                                sp.GetService<IZsLogger>())
                            );

                        services.AddScoped<IMessageProcessor, MessageProcessor>(sp => 
                            new MessageProcessor(
                                sp.GetService<IConfiguration>(),
                                sp.GetService<IMessenger>(),
                                sp.GetService<IContextFactory>(),
                                sp.GetService<IZsLogger>())
                            );

                        services.AddScoped<IScheduler, Scheduler>(sp =>
                            new Scheduler(
                                sp.GetService<IConfiguration>(),
                                sp.GetService<IZsLogger>())
                            );
                        services.AddScoped<ICommandManager, CommandManager>(sp =>
                            new CommandManager(
                                sp.GetService<IContextFactory<BotContext>>(), 
                                sp.GetService<IZsLogger>())
                            );
                        
                        services.AddScoped<IItemsWithRawDataRepository<Chat, int>, ItemsWithRawDataRepository<Chat, int>>(sp =>
                            new ItemsWithRawDataRepository<Chat, int>(
                                sp.GetService<IContextFactory<BotContext>>())
                            );
                        services.AddScoped<IItemsWithRawDataRepository<User, int>, ItemsWithRawDataRepository<User, int>>(sp =>
                            new ItemsWithRawDataRepository<User, int>(
                                sp.GetService<IContextFactory<BotContext>>())
                            );
                        services.AddScoped<IItemsWithRawDataRepository<Message, int>, ItemsWithRawDataRepository<Message, int>>(sp =>
                            new ItemsWithRawDataRepository<Message, int>(
                                sp.GetService<IContextFactory<BotContext>>())
                            );


                        services.AddSingleton<IHostedService, ChatAdmin>(sp =>
                            ActivatorUtilities.CreateInstance<ChatAdmin>(sp));
                    });

                await builder.RunConsoleAsync();
            }
            catch (Exception ex)
            {
                _reloadCounter++;
                Console.WriteLine($"\n\n{ex}\nMessage:\n{ex.Message}"
                                + $"\n\nStackTrace:\n{ex.StackTrace}");

                if (_reloadCounter < 3)
                {
                    Thread.Sleep(1000);
                    await ServiceLoader(configPath);
                }

                Console.ReadLine();
            }
        }
    }
}
