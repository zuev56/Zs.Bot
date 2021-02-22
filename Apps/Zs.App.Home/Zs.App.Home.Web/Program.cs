using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Serilog;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Zs.App.Home.Web
{
    public class Program
    {
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
                
                var configuration = new ConfigurationBuilder()
                    .AddJsonFile(args[0], optional: false, reloadOnChange: true)
                    .Build();

                Log.Logger = new LoggerConfiguration()
                    .ReadFrom.Configuration(configuration, "Serilog")
                    .CreateLogger();

                await CreateHostBuilder(args).Build().RunAsync();
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

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureLogging(logging => logging.AddSerilog())
                .ConfigureAppConfiguration((hostingContext, config) =>
                {
                    config.AddJsonFile(args[0], optional: false, reloadOnChange: true);
                })
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                    //webBuilder.UseUrls(
                    //    "http://localhost:5601",
                    //    "http://192.168.1.11:5601");
                });
    }
}
