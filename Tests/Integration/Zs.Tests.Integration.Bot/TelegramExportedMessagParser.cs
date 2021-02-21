using System;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Zs.Bot.Data;
using Zs.Bot.Data.Abstractions;
using Zs.Bot.Data.Models;

namespace Zs.Tests.Integration.Bot
{
    [TestClass]
    public class TelegramExportedMessageParser : DataBaseClient
    {
        


        [TestMethod]
        public async Task Parse_Test()
        {
            try
            {
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private Chat GetChat()
        {
            return new Chat()
            {
                Id = 1,
                Name = "Message",
                ChatTypeId = "GROUP",
                RawData = "{"
                        +     "\"Id\": -1001259605660,"
                        +     "\"Type\": 3,"
                        +     "\"Title\": \"ZuevTestGroup\","
                        +     "\"AllMembersAreAdministrators\": false"
                        + "}",
                UpdateDate = DateTime.Now,
                InsertDate = DateTime.Now
            };
        }
    }
}
