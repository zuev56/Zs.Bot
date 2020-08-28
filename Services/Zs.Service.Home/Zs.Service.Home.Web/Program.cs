using System;
using System.IO;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace Zs.Service.HomeAPI
{
    public class Program
    {
        public static void Main(string[] args)
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

                CreateHostBuilder(args).Build().Run();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\n\n{ex}\nMessage:"
                                + $"\n{ex.Message}"
                                + $"\n\nStackTrace:\n{ex.StackTrace}");
                Console.ReadKey();
            }
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
            .ConfigureAppConfiguration((hostingContext, config) =>
            {
                config.AddJsonFile(
                    args[0],
                    optional: false, // is required
                    reloadOnChange: true);
            })
            .ConfigureWebHostDefaults(webBuilder =>
            {
                webBuilder.UseStartup<Startup>();
            });
    }
}
