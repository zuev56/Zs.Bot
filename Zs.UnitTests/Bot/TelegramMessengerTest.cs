using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Telegram.Bot.Types;
using Zs.Bot.Helpers;
using Zs.Bot.Model.Db;
using Zs.Bot.Telegram;

namespace Zs.UnitTest.Bot
{
    [TestClass]
    public class TelegramMessengerTest
    {
        private Zs.Bot.Modules.Messaging.IMessenger _messenger;

        public TelegramMessengerTest()
        {
            var configuration = new ConfigurationBuilder().AddJsonFile(@"M:\PrivateBotConfiguration.json", true, true).Build();

            string _botToken = configuration["BotToken"];


            //IWebProxy webProxy = new WebProxy(_proxySocket, true);
            //webProxy.Credentials = new NetworkCredential(_proxyUserName, _proxyPassword);

            _messenger = new TelegramMessenger(_botToken);
            _messenger.MessageReceived += (_) => HeavyMethod(10);
        }


        [TestMethod]
        public async Task SendMessageTest_Test()
        {
            try
            {
                Exception exception = null;
                await Task.Run(async() =>
                {
                    await Task.Delay(10000);
                    for (int i = 0; i < 20; i++)
                    {
                        _messenger.AddMessageToOutbox(GetChat(), $"TestMessage_{i}_q");
                    }
                    await Task.Delay(30000);
                }).ContinueWith(task => exception = task.Exception);

                if (exception != null)
                    throw exception;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        [TestMethod]
        public async Task ReceiveMessageTest_Test()
        {
            try
            {
                Exception exception = null;
                if (_messenger is TelegramMessenger tgMessenger)
                {
                    var privateField = typeof(TelegramMessenger).GetFields(BindingFlags.NonPublic | BindingFlags.Instance)
                                                                .First(f => f.Name == "_inputMessageBuffer");
                    var privateInputBuffer = (Buffer<TgMessage>)privateField.GetValue(tgMessenger);
                    await Task.Run(async () =>
                    {
                        await Task.Delay(10000);
                        for (int i = 0; i < 200; i++)
                        {
                            var message = GenerateTgMessage($"TestMessage_{i}_q");

                            privateInputBuffer.Enqueue(new TgMessage(message)); //tgMessenger.TestMessageToInbox(message);
                        }
                        await Task.Delay(40000);
                    }).ContinueWith(task => exception = task.Exception);
                    
                    if (exception != null)
                        throw exception;
                }
                else
                    throw new InvalidOperationException();
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        private Message GenerateTgMessage(string messageText)
        {
            string rawMessage ="{"
                             + "  \"MessageId\": 5804,                    "
                             + "  \"From\": {                             "
                             + "                \"Id\": 210281448,        "
                             + "    \"IsBot\": false,                     "
                             + "    \"FirstName\": \"Сергей\",            "
                             + "    \"LastName\": \"Зуев\",               "
                             + "    \"Username\": \"zuev56\",             "
                             + "    \"LanguageCode\": \"ru\"              "
                             + "  },                                      "
                             + "  \"Date\": \"2020-06-02T13:55:17Z\",     "
                             + "  \"Chat\": {                             "
                             + "    \"Id\": -1001259605660,               "
                             + "    \"Type\": 3,                          "
                             + "    \"Title\": \"ZuevTestGroup\",         "
                             + "    \"AllMembersAreAdministrators\": false"
                             + "  },                                      "
                             + "  \"IsForwarded\": false,                 "
                             + "  \"ForwardFromMessageId\": 0,            "
                             + "  \"Text\": \"/help\",                    "
                             + "  \"Entities\": [                         "
                             + "    {                                     "
                             + "      \"Type\": 2,                        "
                             + "      \"Offset\": 0,                      "
                             + "      \"Length\": 5                       "
                             + "    }                                     "
                             + "  ],                                      "
                             + "  \"EntityValues\": [                     "
                             + "    \"/help\"                             "
                             + "  ],                                      "
                             + "  \"DeleteChatPhoto\": false,             "
                             + "  \"GroupChatCreated\": false,            "
                             + "  \"SupergroupChatCreated\": false,       "
                             + "  \"ChannelChatCreated\": false,          "
                             + "  \"MigrateToChatId\": 0,                 "
                             + "  \"MigrateFromChatId\": 0,               "
                             + "  \"Type\": 1                             "
                             + "}                                         ";
            var tgMessage = System.Text.Json.JsonSerializer.Deserialize<Message>(rawMessage);
            tgMessage.Text = messageText;

            return tgMessage;
        }

        private IChat GetChat()
        {
            return new DbChat()
            {
                ChatId = 1,
                ChatName = "Message",
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

        private void HeavyMethod(byte heavyLevel)
        {
            if (heavyLevel > 50)
                heavyLevel = 50;

            int d = heavyLevel * 10000;
            int count = 0;
            long a = 2;
            while (count < d)
            {
                long b = 2;
                int prime = 1;
                while (b * b <= a)
                {
                    if (a % b == 0)
                    {
                        prime = 0;
                        break;
                    }
                    b++;
                }
                if (prime > 0)
                {
                    count++;
                }
                a++;
            }
        }
    }
}
