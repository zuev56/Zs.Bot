using System.IO;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Zs.Bot.Data;
using Zs.Bot.Data.Factories;
using Zs.Common.Abstractions;

namespace Zs.Tests.Integration.Bot
{
    [TestClass]
    public class DataBaseClient
    {
        protected static IContextFactory<BotContext> _contextFactory;
        protected static string _connectionString;

        static DataBaseClient() => Init(null);

        public static void Init(TestContext testContext)
        {
            var solutionDir = Common.Extensions.Path.TryGetSolutionPath();
            var configuration = new ConfigurationBuilder().AddJsonFile(Path.Combine(solutionDir,"PrivateConfiguration.json"), true, true).Build();
            _connectionString = configuration.GetConnectionString("Default");

            var optionsBuilder = new DbContextOptionsBuilder<BotContext>();
            optionsBuilder.UseNpgsql(_connectionString);
            optionsBuilder.EnableSensitiveDataLogging(true);
            optionsBuilder.EnableDetailedErrors(true);
            _contextFactory = new BotContextFactory(optionsBuilder.Options);

            //DbInitializer.Initialize(_contextFactory.GetContext());
        }

        [ClassCleanup]
        public static void TestCleanup()
        {
        }


    }
}
