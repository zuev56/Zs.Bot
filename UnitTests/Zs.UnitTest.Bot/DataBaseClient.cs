using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Zs.Bot.Model;
using Zs.Bot.Model.Db;
using Zs.Common.Abstractions;

namespace Zs.UnitTest.Bot
{
    [TestClass]
    public class DataBaseClient
    {
        protected static IContextFactory<ZsBotDbContext> _contextFactory;
        protected static string _connectionString;

        static DataBaseClient() => Init(null);

        public static void Init(TestContext testContext)
        {
            var configuration = new ConfigurationBuilder().AddJsonFile(@"M:\PrivateBotConfiguration.json", true, true).Build();
            _connectionString = configuration.GetConnectionString("ChatAdmin");

            var optionsBuilder = new DbContextOptionsBuilder<ZsBotDbContext>();
            optionsBuilder.UseNpgsql(_connectionString);
            optionsBuilder.EnableSensitiveDataLogging(true);
            optionsBuilder.EnableDetailedErrors(true);
            _contextFactory = new ContextFactory(optionsBuilder.Options);
        }

        [ClassCleanup]
        public static void TestCleanup()
        {
        }


    }
}
