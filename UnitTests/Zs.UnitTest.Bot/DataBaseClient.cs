using System.IO;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Zs.Bot.Model;
using Zs.Bot.Model.Data;
using Zs.Bot.Model.Factories;
using Zs.Common.Abstractions;

namespace Zs.UnitTest.Bot
{
    [TestClass]
    public class DataBaseClient
    {
        protected static IContextFactory<BotContext> _contextFactory;
        protected static string _connectionString;

        static DataBaseClient() => Init(null);

        public static void Init(TestContext testContext)
        {
            var solutionDir = Common.Helpers.Path.TryGetSolutionPath();
            var configuration = new ConfigurationBuilder().AddJsonFile(Path.Combine(solutionDir,"PrivateConfiguration.json"), true, true).Build();
            _connectionString = configuration.GetConnectionString("BotTestCF");

            var optionsBuilder = new DbContextOptionsBuilder<BotContext>();
            optionsBuilder.UseNpgsql(_connectionString);
            optionsBuilder.EnableSensitiveDataLogging(true);
            optionsBuilder.EnableDetailedErrors(true);
            _contextFactory = new ContextFactory(optionsBuilder.Options);

            //DbInitializer.Initialize(_contextFactory.GetContext());
        }

        [ClassCleanup]
        public static void TestCleanup()
        {
        }


    }
}
