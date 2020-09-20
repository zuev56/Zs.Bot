using System;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Zs.Bot.Model;
using Zs.Bot.Model.Abstractions;

namespace Zs.UnitTest.Bot
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

        private IChat GetChat()
        {
            return new Chat()
            {
                Id = 1,
                Name = "Message",
                ChatTypeCode = "GROUP",
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
