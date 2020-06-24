using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using Zs.Bot.Model.Db;
using Zs.Service.ChatAdmin;

namespace Zs.UnitTest.Services.ChatAdmin
{
    [TestClass]
    public class MessageProcessorTest : DataBaseClient
    {


        [TestMethod]
        public void ProcessGroupMessageTest()
        {
            try
            {
                var messageProcessor = new MessageProcessor(0);
                messageProcessor.TestInit();

                messageProcessor.ProcessGroupMessage(GetTestMessage());
            }
            catch (Exception ex)
            {
                throw;
            }

        }



        private DbMessage GetTestMessage()
        {
            return new DbMessage()
            {
                MessageId = 8,
                ChatId = 4,
                UserId = 2,
                MessengerCode = "TG",
                MessageTypeCode = "TXT",
                MessageText = "test",
                IsSucceed = true,
                RawData = "test",
                IsDeleted = false,
                InsertDate = DateTime.Now,
                UpdateDate = DateTime.Now
            };
        }
    }
}
