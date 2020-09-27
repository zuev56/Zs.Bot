using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Zs.Bot;
using Zs.Bot.Messenger.Telegram;
using Zs.Bot.Model.Data;
using Zs.Bot.Model.Factories;
using Zs.Bot.Modules.Logging;
using Zs.Bot.Modules.Messaging;
using Zs.Common.Abstractions;
using Zs.Common.Modules.Connectors;
using Zs.Service.ChatAdmin.Abstractions;
using Zs.Service.ChatAdmin.Data;
using ChatAdminContextFactory = Zs.Service.ChatAdmin.Data.ContextFactory;

namespace Zs.Service.ChatAdmin
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

                        services.AddSingleton<IConnectionAnalyser, ConnectionAnalyser>(sp =>
                        {
                            var ca = new ConnectionAnalyser(sp.GetService<IZsLogger>(),
                                "https://vk.com/", "https://yandex.ru/", "https://www.google.ru/");
                            if (hostContext.Configuration["Proxy:Socket"] != null)
                                ca.InitializeProxy(hostContext.Configuration["Proxy:Socket"],
                                    hostContext.Configuration["Proxy:Login"],
                                    hostContext.Configuration["Proxy:Password"]);
                            return ca;
                        });

                        services.AddSingleton<IMessenger, TelegramMessenger>(sp => 
                            new TelegramMessenger(
                                hostContext.Configuration["BotToken"],
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

                        services.AddSingleton<IHostedService, ChatAdmin>(sp =>
                            ActivatorUtilities.CreateInstance<ChatAdmin>(sp));
                });

                await builder.RunConsoleAsync().ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _reloadCounter++;
                Console.WriteLine($"\n\n{ex}\nMessage:\n{ex.Message}"
                                + $"\n\nStackTrace:\n{ex.StackTrace}");

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
