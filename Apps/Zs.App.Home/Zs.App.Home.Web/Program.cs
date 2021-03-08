using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Serilog;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Zs.App.Home.Web
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            try
            {
                var mainConfigPath = System.IO.Path.Combine(Directory.GetCurrentDirectory(), "appsettings.json");
                if (!File.Exists(mainConfigPath))
                    throw new Exception("Configuration file appsettings.json is not found in the application directory");

                var appsettings = new ConfigurationBuilder().AddJsonFile(mainConfigPath, optional: false, reloadOnChange: true).Build();

                Log.Logger = new LoggerConfiguration()
                    .ReadFrom.Configuration(appsettings, "Serilog")
                    .CreateLogger();

                await CreateHostBuilder(args, appsettings).Build().RunAsync();
            }
            catch (Exception ex)
            {
                TrySaveFailInfo($"\n\n{ex}\nMessage:\n{ex.Message}\n\nStackTrace:\n{ex.StackTrace}");
                Log.Logger.Fatal(ex, "Zs.App.Home fault");

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

        public static IHostBuilder CreateHostBuilder(string[] args, IConfigurationRoot appsettings)
        {           
            return Host.CreateDefaultBuilder(args)
                .ConfigureLogging(logging => logging.AddSerilog())
                .ConfigureAppConfiguration((hostingContext, config) =>
                {
                    foreach (var arg in args)
                    {
                        if (!File.Exists(arg))
                            throw new FileNotFoundException($"Wrong configuration path:\n{arg}");

                        config.AddJsonFile(arg, optional: true, reloadOnChange: true);
                    }

                })
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    string[] urls = appsettings.GetSection("UseUrls").Get<string[]>();
                    if (urls?.Any() == true)
                        webBuilder.UseUrls(appsettings.GetSection("UseUrls").Get<string[]>());

                    webBuilder.UseStartup<Startup>();
                });
        }
            
    }
}
