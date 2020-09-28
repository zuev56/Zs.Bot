//using System;
//using System.Threading.Tasks;
//using Microsoft.VisualStudio.TestTools.UnitTesting;
//using Zs.Bot;
//using Zs.Bot.Model.Db;
//using Zs.Bot.Services.Command;
//using Zs.Bot.Services.Logging;

//namespace Zs.UnitTest.Bot
//{
//    [TestClass]
//    public class CommandManagerTests : DataBaseClient
//    {
//        private static ICommandManager _commandManager;
//        private static Func<string, ICommand> _getDbCommand;

//        [ClassInitialize()]
//        public static void Init(TestContext testContext)
//        {
//            _commandManager = new CommandManager(_contextFactory, new Logger(_contextFactory));
//            _getDbCommand = ((CommandManager)_commandManager).GetDbCommand;
//        }

//        /// <summary> Создание команды, помещение её в очередь и вызов из очереди на обработку
//        /// НЕ тестируется обработка команды!</summary>
//        [TestMethod]
//        public async Task CommandQueue_Test()
//        {
//            try
//            {
//                var userMessage = DbEntityFactory.NewMessage(@"/test p1 p2, p3;  p4");
//                var botCommand = await BotCommand.ParseMessageAsync(userMessage, _getDbCommand);

//                var enqueueResult = _commandManager.EnqueueCommand(botCommand);

//                // На этом этапе сообщение передаётся в другой поток на обработку и тест считаем выполненным
//                Assert.IsTrue(enqueueResult);
//            }
//            catch (Exception ex)
//            {
//                throw ex;
//            }
//        }

//        /// <summary> Создание и обработка команды без аргументов </summary>
//        [TestMethod]
//        public async Task CommandProcessNoArgs_Test()
//        {
//            try
//            {
//                var userMessage = DbEntityFactory.NewMessage("/Test");
//                var botCommand = await BotCommand.ParseMessageAsync(userMessage, _getDbCommand);

//                var result = ((CommandManager)_commandManager).RunCommand(botCommand);

//                Assert.AreNotEqual(result, $"Command '{botCommand.Name}' running failed!");
//            }
//            catch (Exception ex)
//            {
//                throw;
//            }
//        }

//        /// <summary> Создание и обработка команды c аргументами пользователя </summary>
//        [TestMethod]
//        public async Task CommandProcessUserArgs_Test()
//        {
//            try
//            {
//                var userMessage = DbEntityFactory.NewMessage("/GetUserStatistics 20 \"'2019-10-29 00:00:00.0+03'\", \"'2019-10-30 00:00:00.0+03'\"; ");
//                var botCommand = await BotCommand.ParseMessageAsync(userMessage, _getDbCommand);

//                var result = ((CommandManager)_commandManager).RunCommand(botCommand);

//                Assert.AreNotEqual(result, $"Command '{botCommand.Name}' running failed!");
//            }
//            catch (Exception ex)
//            {
//                throw;
//            }
//        }

//        /// <summary> Создание и обработка команды c аргументами по умолчанию </summary>
//        [TestMethod]
//        public async Task CommandProcessDefaultArgs_Test()
//        {
//            try
//            {
//                var userMessage = DbEntityFactory.NewMessage("/GetUserStatistics");
//                var botCommand = await BotCommand.ParseMessageAsync(userMessage, _getDbCommand);

//                var result = ((CommandManager)_commandManager).RunCommand(botCommand);

//                Assert.AreNotEqual(result, $"Command '{botCommand.Name}' running failed!");
//            }
//            catch (Exception ex)
//            {
//                throw;
//            }
//        }
        
//        [TestMethod]
//        public void GetCommandsForRole_Test()
//        {
//            try
//            {
//                var commands0 = _commandManager.GetDbCommands("ADMIN");
//                var commands1 = _commandManager.GetDbCommands("MODERATOR");
//                var commands2 = _commandManager.GetDbCommands("USER");

//                Assert.IsTrue(commands0.Count > 0);
//                Assert.IsTrue(commands1.Count > 0);
//                Assert.IsTrue(commands2.Count > 0);
//            }
//            catch (Exception ex)
//            {
//                throw;
//            }
//        }
//    }
//}
