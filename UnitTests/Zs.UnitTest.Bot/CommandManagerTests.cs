using System;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Zs.Bot;
using Zs.Bot.Modules.Command;

namespace Zs.UnitTest.Bot
{
    [TestClass]
    public class CommandManagerTests : DataBaseClient
    {
        public CommandManagerTests()
        {
        }

        /// <summary> Создание команды, помещение её в очередь и вызов из очереди на обработку
        /// НЕ тестируется обработка команды!</summary>
        [TestMethod]
        public async Task CommandQueue_Test()
        {
            try
            {
                var userMessage = DbEntityFactory.NewMessage(@"/test p1 p2, p3;  p4");
                var botCommand = await BotCommand.ParseMessageAsync(userMessage);

                var enqueueResult = new CommandManager().EnqueueCommand(botCommand);

                // На этом этапе сообщение передаётся в другой поток на обработку и тест считаем выполненным
                Assert.IsTrue(enqueueResult);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary> Создание и обработка команды без аргументов </summary>
        [TestMethod]
        public async Task CommandProcessNoArgs_Test()
        {
            try
            {
                var userMessage = DbEntityFactory.NewMessage("/Test");
                var botCommand = await BotCommand.ParseMessageAsync(userMessage);

                var result = new CommandManager().RunCommand(botCommand);

                Assert.AreNotEqual(result, $"Command '{botCommand.Name}' running failed!");
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        /// <summary> Создание и обработка команды c аргументами пользователя </summary>
        [TestMethod]
        public async Task CommandProcessUserArgs_Test()
        {
            try
            {
                var userMessage = DbEntityFactory.NewMessage("/GetUserStatistics 20 \"'2019-10-29 00:00:00.0+03'\", \"'2019-10-30 00:00:00.0+03'\"; ");
                var botCommand = await BotCommand.ParseMessageAsync(userMessage);

                var result = new CommandManager().RunCommand(botCommand);

                Assert.AreNotEqual(result, $"Command '{botCommand.Name}' running failed!");
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        /// <summary> Создание и обработка команды c аргументами по умолчанию </summary>
        [TestMethod]
        public async Task CommandProcessDefaultArgs_Test()
        {
            try
            {
                var userMessage = DbEntityFactory.NewMessage("/GetUserStatistics");
                var botCommand = await BotCommand.ParseMessageAsync(userMessage);

                var result = new CommandManager().RunCommand(botCommand);

                Assert.AreNotEqual(result, $"Command '{botCommand.Name}' running failed!");
            }
            catch (Exception ex)
            {
                throw;
            }
        }
        
        [TestMethod]
        public void GetCommandsForRole_Test()
        {
            try
            {
                var commands0 = CommandManager.GetDbCommands("ADMIN");
                var commands1 = CommandManager.GetDbCommands("MODERATOR");
                var commands2 = CommandManager.GetDbCommands("USER");

                Assert.IsTrue(commands0.Count > 0);
                Assert.IsTrue(commands1.Count > 0);
                Assert.IsTrue(commands2.Count > 0);
            }
            catch (Exception ex)
            {
                throw;
            }
        }
    }
}
