using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Zs.Bot.Model.Db;
using Zs.Common.Abstractions;
using Zs.Service.ChatAdmin.Model;
using BotContextFactory = Zs.Bot.Model.ContextFactory;
using ChatAdminContextFactory = Zs.Service.ChatAdmin.Model.ContextFactory;

namespace Zs.UnitTest.Services.ChatAdmin
{
    [TestClass]
    public class DataBaseClient
    {
        protected static IContextFactory<ZsBotDbContext> _botContextFactory;
        protected static IContextFactory<ChatAdminDbContext> _caContextFactory;
        protected static string _connectionString;
        static DataBaseClient() => Init(null);
        
        public static void Init(TestContext testContext)
        {
            var configuration = new ConfigurationBuilder().AddJsonFile(@"M:\PrivateBotConfiguration.json", true, true).Build();
            _connectionString = configuration.GetConnectionString("ChatAdmin");

            var botOptionsBuilder = new DbContextOptionsBuilder<ZsBotDbContext>();
            botOptionsBuilder.UseNpgsql(_connectionString);
            botOptionsBuilder.EnableSensitiveDataLogging(true);
            botOptionsBuilder.EnableDetailedErrors(true);
            _botContextFactory = new BotContextFactory(botOptionsBuilder.Options);
            
            var caOptionsBuilder = new DbContextOptionsBuilder<ChatAdminDbContext>();
            caOptionsBuilder.UseNpgsql(_connectionString);
            caOptionsBuilder.EnableSensitiveDataLogging(true);
            caOptionsBuilder.EnableDetailedErrors(true);
            _caContextFactory = new ChatAdminContextFactory(caOptionsBuilder.Options);
        }
        
        [ClassCleanup]
        public static void TestCleanup()
        {
        }


    }
}
