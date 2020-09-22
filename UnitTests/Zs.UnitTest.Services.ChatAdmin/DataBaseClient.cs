using System.IO;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Zs.Bot.Model.Data;
using Zs.Service.ChatAdmin.Abstractions;
using Zs.Service.ChatAdmin.Data;
using ChatAdminContextFactory = Zs.Service.ChatAdmin.Data.ContextFactory;

namespace Zs.UnitTest.Services.ChatAdmin
{
    [TestClass]
    public class DataBaseClient
    {
        protected static IContextFactory _contextFactory;
        protected static string _connectionString;
        static DataBaseClient() => Init(null);
        
        public static void Init(TestContext testContext)
        {
            var solutionDir = Common.Helpers.Path.TryGetSolutionPath();
            var configuration = new ConfigurationBuilder().AddJsonFile(Path.Combine(solutionDir,"PrivateConfiguration.json"), true, true).Build();
            _connectionString = configuration.GetConnectionString("ChatAdmin");

            var botOptionsBuilder = new DbContextOptionsBuilder<BotContext>();
            botOptionsBuilder.UseNpgsql(_connectionString);
            botOptionsBuilder.EnableSensitiveDataLogging(true);
            botOptionsBuilder.EnableDetailedErrors(true);

            var caOptionsBuilder = new DbContextOptionsBuilder<ChatAdminContext>();
            caOptionsBuilder.UseNpgsql(_connectionString);
            caOptionsBuilder.EnableSensitiveDataLogging(true);
            caOptionsBuilder.EnableDetailedErrors(true);

            _contextFactory = new ChatAdminContextFactory(
                botOptionsBuilder.Options, caOptionsBuilder.Options);
        }
        
        [ClassCleanup]
        public static void TestCleanup()
        {
        }


    }
}
