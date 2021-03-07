using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Zs.Common.Extensions;
using Xunit;
using Moq;
using Zs.Bot.Services.Messaging;

namespace Zs.Tests.Unit.ChatAdmin
{
    class MessageProcessorTest
    {

        [Fact]
        public void ProcessGroupMessageTest()
        {
            var solutionDir = Common.Extensions.Path.TryGetSolutionPath();
            var configuration = new ConfigurationBuilder().AddJsonFile(System.IO.Path.Combine(solutionDir, "PrivateConfiguration.json"), true, true).Build();

            var mockConfig = new Mock<IConfiguration>();
            var mockMessenger = new Mock<IMessenger>();
             
            // Продолжить работу по этому описанию https://m.habr.com/ru/post/531106/

            //Запустить ChatAdmin и проверить удаление банов(предупреждений) при отключении интернета

            //MessageProcessor(
            //IConfiguration configuration,
            //IMessenger messenger,
            //IContextFactory contextFactory,
            //ILogger < MessageProcessor > logger)

            // Arrange


            // Act


            // Assert

        }

        public void ResetLimitsTest()
        {


        }

        public void SetInternetRepairDate()
        {


        }
    }
}
