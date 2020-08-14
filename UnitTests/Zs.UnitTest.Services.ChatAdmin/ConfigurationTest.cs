using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using Zs.Service.ChatAdmin;

namespace Zs.UnitTest.Services.ChatAdmin
{
    [TestClass]
    public class ConfigurationTest
    {
        [TestMethod]
        public void PrepareConnectionStringTest()
        {
            var config = new Configuration(@"M:\PrivateBotConfiguration.json");

            Assert.IsNotNull(config["BotToken"]);
        }
    }
}
