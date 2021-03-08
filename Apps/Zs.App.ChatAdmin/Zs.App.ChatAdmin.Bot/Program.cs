using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Zs.App.ChatAdmin.Abstractions;
using Zs.App.ChatAdmin.Data;
using Zs.App.ChatAdmin.Model;
using Zs.Bot.Data;
using Zs.Bot.Data.Abstractions;
using Zs.Bot.Data.Factories;
using Zs.Bot.Data.Models;
using Zs.Bot.Data.Repositories;
using Zs.Bot.Messenger.Telegram;
using Zs.Bot.Services.Commands;
using Zs.Bot.Services.DataSavers;
using Zs.Bot.Services.Messaging;
using Zs.Common.Abstractions;
using Zs.Common.Extensions;
using Zs.Common.Services.Abstractions;
using Zs.Common.Services.Connection;
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
                var mainConfigPath = System.IO.Path.Combine(Directory.GetCurrentDirectory(), "appsettings.json");
                if (!File.Exists(mainConfigPath))
                    throw new Exception("Configuration file appsettings.json is not found in the application directory");

                var configurationBuilder = new ConfigurationBuilder().AddJsonFile(mainConfigPath, optional: false, reloadOnChange: true);

                foreach (var arg in args)
                {
                    if (!File.Exists(arg))
                        throw new FileNotFoundException($"Wrong configuration path:\n{arg}");

                    configurationBuilder.AddJsonFile(arg, optional: true, reloadOnChange: true);
                }

                await ServiceLoader(configurationBuilder.Build());
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

        public static async Task ServiceLoader(IConfiguration configuration)
        {
            Serilog.Log.Logger = new LoggerConfiguration()
                .ReadFrom.Configuration(configuration, "Serilog")
                .CreateLogger();

            try
            {
                var builder = new HostBuilder()
                    .ConfigureAppConfiguration((hostingContext, config) => config.AddConfiguration(configuration))
                    .ConfigureLogging(logging => logging.AddSerilog())
                    .ConfigureServices((hostContext, services) =>
                    {
                        services.AddDbContext<ChatAdminContext>(options =>
                            options.UseNpgsql(hostContext.Configuration.GetSecretValue("ConnectionStrings:Default")));

                        services.AddDbContext<BotContext>(options =>
                            options.UseNpgsql(hostContext.Configuration.GetSecretValue("ConnectionStrings:Default")));

                        // For repositories
                        services.AddSingleton<IContextFactory<ChatAdminContext>, ChatAdminContextFactory>();
                        services.AddSingleton<IContextFactory<BotContext>, BotContextFactory>();

                        services.AddScoped<IConnectionAnalyser, ConnectionAnalyser>(sp =>
                        {
                            var ca = new ConnectionAnalyser(sp.GetService<ILogger<ConnectionAnalyser>>(), 
                                configuration.GetSection("ConnectionAnalyser:Urls").Get<string[]>());
                            if (hostContext.Configuration.GetSection("Proxy:UseProxy")?.Get<bool>() == true)
                                ca.InitializeProxy(hostContext.Configuration["Proxy:Socket"],
                                    hostContext.Configuration.GetSecretValue("Proxy:Login"),
                                    hostContext.Configuration.GetSecretValue("Proxy:Password"));
                            return ca;
                        });

                        services.AddScoped<IMessenger, TelegramMessenger>(sp => 
                            new TelegramMessenger(
                                hostContext.Configuration.GetSecretValue("BotToken"),
                                sp.GetService<IItemsWithRawDataRepository<Chat, int>>(),
                                sp.GetService<IItemsWithRawDataRepository<User, int>>(),
                                sp.GetService<IItemsWithRawDataRepository<Message, int>>(),
                                sp.GetService<IMessageDataSaver>(),
                                sp.GetService<ICommandManager>(),
                                sp.GetService<IConnectionAnalyser>().WebProxy,
                                sp.GetService<ILogger<TelegramMessenger>>())
                            );

                        services.AddScoped<IMessageDataSaver, MessageDataDBSaver>();
                        services.AddScoped<IMessageProcessor, MessageProcessor>(sp =>
                            new MessageProcessor(
                                sp.GetService<IConfiguration>(),
                                sp.GetService<IMessenger>(),
                                sp.GetService<IItemsWithRawDataRepository<Chat, int>>(),
                                sp.GetService<IItemsWithRawDataRepository<User, int>>(),
                                sp.GetService<IItemsWithRawDataRepository<Message, int>>(),
                                sp.GetService<IRepository<Ban, int>>(),
                                sp.GetService<ILogger<MessageProcessor>>())
                            );
                        services.AddScoped<IScheduler, Scheduler>();
                        services.AddScoped<ICommandManager, CommandManager>(sp =>
                            new CommandManager(
                                hostContext.Configuration.GetSecretValue("ConnectionStrings:Default"),
                                sp.GetService<IRepository<Command, string>>(),
                                sp.GetService<IRepository<UserRole, string>>(),
                                sp.GetService<IItemsWithRawDataRepository<User, int>>(),
                                sp.GetService<ILogger<CommandManager>>())
                            );

                        services.AddScoped<IRepository<Ban, int>, CommonRepository<ChatAdminContext, Ban, int>>();
                        //services.AddScoped<IRepository<Bot.Data.Models.Log, int>, CommonRepository<BotContext, Bot.Data.Models.Log, int>>();
                        services.AddScoped<IRepository<Command, string>, CommonRepository<BotContext, Command, string>>();
                        services.AddScoped<IRepository<UserRole, string>, CommonRepository<BotContext, UserRole, string>>();
                        services.AddScoped<IItemsWithRawDataRepository<Chat, int>, ItemsWithRawDataRepository<BotContext, Chat, int>>();
                        services.AddScoped<IItemsWithRawDataRepository<User, int>, ItemsWithRawDataRepository<BotContext, User, int>>();
                        services.AddScoped<IItemsWithRawDataRepository<Message, int>, ItemsWithRawDataRepository<BotContext, Message, int>>();

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
                    await ServiceLoader(configuration);
                }

                Console.ReadLine();
            }
        }
    }
}
