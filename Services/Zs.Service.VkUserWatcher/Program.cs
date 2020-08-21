﻿using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Zs.Bot.Model.Db;

namespace Zs.Service.VkUserWatcher
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
                //var connectionAnalyser = new ConnectionAnalyser(Logger.GetInstance(), "https://vk.com/", "https://yandex.ru/", "https://www.google.ru/");
                //if (configuration["ProxySocket"] is { })
                //    connectionAnalyser.InitializeProxy(
                //        (string)configuration["ProxySocket"],
                //        (string)configuration["ProxyLogin"],
                //        (string)configuration["ProxyPassword"]
                //        );

                //var messenger = new TelegramMessenger(token, webProxy);

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
                        services.AddDbContext<ZsBotDbContext>(options =>
                            options.UseNpgsql(hostContext.Configuration.GetConnectionString("VkUserWatcher")));
                        
                        services.AddSingleton<IHostedService, UserWatcher>(x =>
                            ActivatorUtilities.CreateInstance<UserWatcher>(x));
                        
                        //services.AddSingleton<IHostedService, ConnectionAnalyser>(x =>
                        //    ActivatorUtilities.CreateInstance<ConnectionAnalyser>(x));
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