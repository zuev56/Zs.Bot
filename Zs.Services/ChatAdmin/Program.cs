using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
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
                    throw new FileNotFoundException("Wrong private configuration path");

                var configText = File.ReadAllText(args[0]);
                var configDictionary = JsonConvert.DeserializeObject<Dictionary<string, string>>(configText);

                await ServiceLoader(configDictionary);
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


        public static async Task ServiceLoader(Dictionary<string,string> config)
        {
            try
            {
                var configuration = new Configuration(
                    "Zs.Service.ChatAdmin.json",
                    dbUser:        config["DbUser"],
                    dbPassword:    config["DbPassword"],
                    botToken:      config["BotToken"],
                    proxySocket:   config.ContainsKey("ProxySocket")   ? config["ProxySocket"]   : null,
                    proxyLogin:    config.ContainsKey("ProxyUser")     ? config["ProxyUser"]     : null,
                    proxyPassword: config.ContainsKey("ProxyPassword") ? config["ProxyPassword"] : null
                    );

                var connectionAnalyser = new ConnectionAnalyser(Logger.GetInstance(), "https://vk.com/", "https://yandex.ru/", "https://www.google.ru/");
                if (config.ContainsKey("ProxySocket"))
                    connectionAnalyser.InitializeProxy(
                        configuration.ProxySocket,
                        configuration.ProxyLogin,
                        configuration.ProxyPassword
                        );

                var messenger = MessengerFactory.ProvideMessenger("Telegram", configuration.BotToken, connectionAnalyser.WebProxy);

                var builder = new HostBuilder()
                    .ConfigureServices((hostContext, services) =>
                    {
                        services.AddSingleton<IHostedService, ChatAdmin>(x =>
                            ActivatorUtilities.CreateInstance<ChatAdmin>(x, configuration, messenger, connectionAnalyser));
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
                    await ServiceLoader(config);
                }

                Console.ReadLine();
            }
        }

    }

}
