using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Zs.Bot.Helpers;
using Zs.Common.Modules.Connectors;
using Zs.Service.ChatAdmin.Factories;

namespace Zs.Service.ChatAdmin
{
    class Program
    {
        private static int _reloadCounter = 0;


        public static async Task Main(string[] args)
        {
            try
            {
                if (args?.Length  != 1)
                    throw new ArgumentException("Wrong number of arguments");

                if (!File.Exists(args[0]))
                    throw new FileNotFoundException("Wrong configuration path");

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
                var configuration = new Configuration(configPath);

                var connectionAnalyser = new ConnectionAnalyser(Logger.GetInstance(), "https://vk.com/", "https://yandex.ru/", "https://www.google.ru/");
                if (configuration["ProxySocket"] is {})
                    connectionAnalyser.InitializeProxy(
                        (string)configuration["ProxySocket"],
                        (string)configuration["ProxyLogin"],
                        (string)configuration["ProxyPassword"]
                        );

                var messenger = MessengerFactory.ProvideMessenger("Telegram", (string)configuration["BotToken"], connectionAnalyser.WebProxy);

                var builder = new HostBuilder()
                    .ConfigureServices((hostContext, services) =>
                    {
                        services.AddSingleton<IHostedService, ChatAdmin>(x =>
                            ActivatorUtilities.CreateInstance<ChatAdmin>(x, configuration, messenger, connectionAnalyser));
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
