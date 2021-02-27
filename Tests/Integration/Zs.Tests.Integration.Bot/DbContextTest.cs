using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Zs.Bot.Data;
using Zs.Common.Enums;
using Zs.Common.Services.Scheduler;

namespace Zs.Tests.Integration.Bot
{
    [TestClass]
    public class DbContextTest : DataBaseClient
    {


        [TestMethod]
        public void ReadResourceTest()
        {
            BotContext.GetOtherSqlScripts();
        }
    }
}
