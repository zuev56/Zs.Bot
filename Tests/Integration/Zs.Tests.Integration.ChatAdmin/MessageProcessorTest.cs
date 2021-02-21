using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using Zs.Bot.Data.Models;

namespace Zs.Tests.Integration.ChatAdmin
{
    [TestClass]
    public class MessageProcessorTest : DataBaseClient
    {


        //[TestMethod]
        //public void ProcessGroupMessageTest()
        //{
        //    try
        //    {
        //var solutionDir = Common.Helpers.Path.TryGetSolutionPath();
        //        var config = new Configuration(@"M:\PrivateBotConfiguration.json");
        //        var messenger = 
        //
        //        var messageProcessor = new MessageProcessor(config, messenger);
        //
        //        messageProcessor.ProcessGroupMessage(GetTestMessage());
        //    }
        //    catch (Exception ex)
        //    {
        //        throw;
        //    }
        //
        //}



        private Message GetTestMessage()
        {
            return new Message()
            {
                Id = 8,
                ChatId = 4,
                UserId = 2,
                MessengerId = "TG",
                MessageTypeId = "TXT",
                Text = "test",
                IsSucceed = true,
                RawData = "test",
                IsDeleted = false,
                InsertDate = DateTime.Now,
                UpdateDate = DateTime.Now
            };
        }
    }
}
