﻿using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Zs.Bot.Model.Db;
using Zs.Service.ChatAdmin;
using Zs.Service.ChatAdmin.Model;

namespace Zs.UnitTest.Services.ChatAdmin
{
    [TestClass]
    public class DataBaseClient
    {
        static DataBaseClient() => Init(null);
        
        [ClassInitialize()]
        public static void Init(TestContext testContext)
        {
            var configuration = new ConfigurationBuilder().AddJsonFile(@"M:\PrivateBotConfiguration.json", true, true).Build();

            var rOptionsBuilder = new DbContextOptionsBuilder<ZsBotDbContext>();
            rOptionsBuilder.UseNpgsql(configuration["ConnectionString"]);
            ZsBotDbContext.Initialize(rOptionsBuilder.Options);

            var caOptionsBuilder = new DbContextOptionsBuilder<ChatAdminDbContext>();
            caOptionsBuilder.UseNpgsql(configuration["ConnectionString"]);
            ChatAdminDbContext.Initialize(caOptionsBuilder.Options);
        }
        
        [ClassCleanup]
        public static void TestCleanup()
        {
        }


    }
}
