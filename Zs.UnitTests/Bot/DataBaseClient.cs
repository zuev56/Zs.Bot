using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Zs.Bot.Model.Db;

namespace Zs.UnitTest.Bot
{
    [TestClass]
    public class DataBaseClient
    {
        private static DbContextOptionsBuilder<ZsBotDbContext> _robotOptionsBuilder;
        protected static readonly Encoding Utf8Encoder = Encoding.GetEncoding(
            "UTF-8",
            new EncoderReplacementFallback(string.Empty),
            new DecoderExceptionFallback()
        );

        static DataBaseClient() => Init(null);

        [ClassInitialize()]
        public static void Init(TestContext testContext)
        {
            var configuration = new ConfigurationBuilder().AddJsonFile(@"M:\PrivateBotConfiguration.json", true, true).Build();

            _robotOptionsBuilder = new DbContextOptionsBuilder<ZsBotDbContext>();
            _robotOptionsBuilder.UseNpgsql(configuration["ConnectionString"]);
            ZsBotDbContext.Initialize(_robotOptionsBuilder.Options);
        }

        [ClassCleanup]
        public static void TestCleanup()
        {
        }


    }
}
