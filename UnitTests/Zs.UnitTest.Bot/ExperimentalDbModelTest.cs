using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Zs.Bot.Model;
using Zs.Common.Extensions;
using ZsBot = Zs.Bot.Model.Bot;

namespace Zs.UnitTest.Bot
{
    [TestClass]
    public class DbModelTest : DataBaseClient
    {
        [TestMethod]
        public void DbModel_SelectTest()
        {
            try
            {
                using (var ctx = _contextFactory.GetContext())
                {
                    Assert.IsNotNull(ctx.Bots.FirstOrDefault());
                    Assert.IsNotNull(ctx.ChatTypes.FirstOrDefault());
                    Assert.IsNotNull(ctx.Chats.FirstOrDefault());
                    Assert.IsNotNull(ctx.UserRoles.FirstOrDefault());
                    Assert.IsNotNull(ctx.Users.FirstOrDefault());
                    Assert.IsNotNull(ctx.Messengers.FirstOrDefault());
                    Assert.IsNotNull(ctx.MessageTypes.FirstOrDefault());
                    Assert.IsNotNull(ctx.Messages.FirstOrDefault());
                    Assert.IsNotNull(ctx.Logs.FirstOrDefault());
                    Assert.IsNotNull(ctx.Commands.FirstOrDefault());
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [TestMethod]
        public void DbModel_UpdateTest()
        {
            try
            {
                using var ctx = _contextFactory.GetContext();

                var bot         = ctx.Bots.FirstOrDefault();
                var chatType    = ctx.ChatTypes.FirstOrDefault();
                var chat        = ctx.Chats.FirstOrDefault();
                var role        = ctx.UserRoles.FirstOrDefault();
                var user        = ctx.Users.FirstOrDefault();
                var messenger   = ctx.Messengers.FirstOrDefault();
                var messageType = ctx.MessageTypes.FirstOrDefault();
                var message     = ctx.Messages.FirstOrDefault();
                var log         = ctx.Logs.FirstOrDefault();
                var command     = ctx.Commands.FirstOrDefault();

                var botId           = bot.Id;
                var chatTypeCode    = chatType.Code;
                var chatId          = chat.Id;
                var roleCode        = role.Code;
                var userId          = user.Id;
                var messengerCode   = messenger.Code;
                var messageTypeCode = messageType.Code;
                var messageId       = message.Id;
                var logId           = log.Id;
                var commandName     = command.Name;

                var newUpdateDate      = DateTime.Now;
                bot.UpdateDate         = newUpdateDate;
                chatType.UpdateDate    = newUpdateDate;
                chat.UpdateDate        = newUpdateDate;
                role.UpdateDate        = newUpdateDate;
                user.UpdateDate        = newUpdateDate;
                messenger.UpdateDate   = newUpdateDate;
                messageType.UpdateDate = newUpdateDate;
                message.UpdateDate     = newUpdateDate;
                log.Message            = $"{newUpdateDate}";
                command.UpdateDate     = newUpdateDate;

                ctx.SaveChanges();

                Assert.IsTrue(newUpdateDate == ctx.Bots.First(b => b.Id == botId).UpdateDate);
                Assert.IsTrue(newUpdateDate == ctx.ChatTypes.First(t => t.Code == chatTypeCode).UpdateDate);
                Assert.IsTrue(newUpdateDate == ctx.Chats.First(c => c.Id == chatId).UpdateDate);
                Assert.IsTrue(newUpdateDate == ctx.UserRoles.First(r => r.Code == roleCode).UpdateDate);
                Assert.IsTrue(newUpdateDate == ctx.Users.First(u => u.Id == userId).UpdateDate);
                Assert.IsTrue(newUpdateDate == ctx.Messengers.First(m => m.Code == messengerCode).UpdateDate);
                Assert.IsTrue(newUpdateDate == ctx.MessageTypes.First(t => t.Code == messageTypeCode).UpdateDate);
                Assert.IsTrue(newUpdateDate == ctx.Messages.First(m => m.Id == messageId).UpdateDate);
                Assert.IsTrue($"{newUpdateDate}" == ctx.Logs.First(l => l.Id == logId).Message);
                Assert.IsTrue(newUpdateDate == ctx.Commands.First(c => c.Name == commandName).UpdateDate);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [TestMethod]
        public void DbModel_FirstInsertTest()
        {
            try
            {
                var testJsonValue = "{\"value\": \"UnitTest1\"}";

                var messenger       = new MessengerInfo   { Code = "U1", Name = "UnitTest1" };
                var bot             = new ZsBot           { Id = -2, MessengerCode = "U1", Name = "UnitTest1", Token = "UnitTest1" };
                var chatType        = new ChatType        { Code = "UNITTEST1", Name = "UnitTest1"};
                var chat            = new Chat            { Id = -2, ChatTypeCode = "UNITTEST1", Name = "UnitTest1", RawData = testJsonValue, RawDataHash = testJsonValue.GetMD5Hash()};
                var role            = new UserRole        { Code = "UNITTEST1", Name = "UnitTest1", Permissions = "[ \"UnitTest1\" ]" };
                var user            = new User            { Id = -2, Name = "UnitTest1", UserRoleCode = "UNITTEST1", IsBot = false, RawData = testJsonValue, RawDataHash = testJsonValue.GetMD5Hash() };
                var messageType     = new MessageType     { Code = "U1", Name = "UnitTest1" };
                var message         = new Message         { Id = -2, MessengerCode = "U1", MessageTypeCode = "U1", ChatId = -2, UserId = -2, RawData = testJsonValue, RawDataHash = testJsonValue.GetMD5Hash() };
                var log             = new Log             { Id = -2, Type = "WARNING", Message = "UnitTest1" };
                var command         = new Command         { Name = "/unittest1", Script = "UnitTest1", Group = "UnitTest1" };

                using (var ctx = _contextFactory.GetContext())
                {
                    if (!ctx.Messengers.Any(m => m.Code == messenger.Code))
                        ctx.Messengers.Add(messenger);

                    if (!ctx.ChatTypes.Any(t => t.Code == chatType.Code))
                        ctx.ChatTypes.Add(chatType);

                    if (!ctx.UserRoles.Any(r => r.Code == role.Code))
                        ctx.UserRoles.Add(role);

                    if (!ctx.MessageTypes.Any(t => t.Code == messageType.Code))
                        ctx.MessageTypes.Add(messageType);

                    if (ctx.ChangeTracker.HasChanges())
                        ctx.SaveChanges();

                    if (!ctx.Bots.Any(b => b.Id == bot.Id))
                    {
                        ctx.Bots.Add(bot);
                        ctx.SaveChanges();
                    }

                    if (!ctx.Chats.Any(c => c.Id == chat.Id))
                    {
                        ctx.Chats.Add(chat);
                        ctx.SaveChanges();
                    }

                    if (!ctx.Users.Any(u => u.Id == user.Id))
                    {
                        ctx.Users.Add(user);
                        ctx.SaveChanges();
                    }

                    if (!ctx.Messages.Any(m => m.Id == message.Id))
                    {
                        ctx.Messages.Add(message);
                        ctx.SaveChanges();
                    }

                    if (!ctx.Logs.Any(l => l.Id == log.Id))
                        ctx.Logs.Add(log);

                    if (!ctx.Commands.Any(c => c.Name == command.Name))
                        ctx.Commands.Add(command);

                    if (ctx.ChangeTracker.HasChanges())
                        ctx.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        [TestMethod]
        public void DbModel_InsertDelete()
        {
            try
            {
                var testJsonValue = "{\"value\": \"UnitTest0\"}";

                var messenger       = new MessengerInfo   { Code = "U0", Name = "UnitTest0" };
                var bot             = new ZsBot           { Id = -1, MessengerCode = "U0", Name = "UnitTest0", Token = "UnitTest0" };
                var chatType        = new ChatType        { Code = "UNITTEST0", Name = "UnitTest0"};
                var chat            = new Chat            { Id = -1, ChatTypeCode = "UNITTEST0", Name = "UnitTest0", RawData = testJsonValue, RawDataHash = testJsonValue.GetMD5Hash()};
                var role            = new UserRole        { Code = "UNITTEST0", Name = "UnitTest0", Permissions = "[ \"UnitTest0\" ]" };
                var user            = new User            { Id = -1, Name = "UnitTest0", UserRoleCode = "UNITTEST0", IsBot = false, RawData = testJsonValue, RawDataHash = testJsonValue.GetMD5Hash() };
                var messageType     = new MessageType     { Code = "U0", Name = "UnitTest0" };
                var message         = new Message         { Id = -1, MessengerCode = "U0", MessageTypeCode = "U0", ChatId = -1, UserId = -1, RawData = testJsonValue, RawDataHash = testJsonValue.GetMD5Hash() };
                var log             = new Log             { Id = -1, Type = "WARNING", Message = "UnitTest0" };
                var command         = new Command         { Name = "/unittest0", Script = "UnitTest0", Group = "UnitTest0" };

                using (var ctx = _contextFactory.GetContext())
                {
                    if (!ctx.Messengers.Any(m => m.Code == messenger.Code))
                        ctx.Messengers.Add(messenger);

                    if (!ctx.ChatTypes.Any(t => t.Code == chatType.Code))
                        ctx.ChatTypes.Add(chatType);

                    if (!ctx.UserRoles.Any(r => r.Code == role.Code))
                        ctx.UserRoles.Add(role);

                    if (!ctx.MessageTypes.Any(t => t.Code == messageType.Code))
                        ctx.MessageTypes.Add(messageType);

                    if (ctx.ChangeTracker.HasChanges())
                        ctx.SaveChanges();

                    if (!ctx.Bots.Any(b => b.Id == bot.Id))
                    {
                        ctx.Bots.Add(bot);
                        ctx.SaveChanges();
                    }

                    if (!ctx.Chats.Any(c => c.Id == chat.Id))
                    {
                        ctx.Chats.Add(chat);
                        ctx.SaveChanges();
                    }

                    if (!ctx.Users.Any(u => u.Id == user.Id))
                    {
                        ctx.Users.Add(user);
                        ctx.SaveChanges();
                    }

                    if (!ctx.Messages.Any(m => m.Id == message.Id))
                    {
                        ctx.Messages.Add(message);
                        ctx.SaveChanges();
                    }

                    if (!ctx.Logs.Any(l => l.Id == log.Id))
                        ctx.Logs.Add(log);

                    if (!ctx.Commands.Any(c => c.Name == command.Name))
                        ctx.Commands.Add(command);

                    if (ctx.ChangeTracker.HasChanges())
                        ctx.SaveChanges();
                }

                // Удаление из БД
                using (var ctx = _contextFactory.GetContext())
                {
                    ctx.Logs.Remove(log);
                    ctx.Commands.Remove(command);
                    ctx.SaveChanges();
                    ctx.Messages.Remove(message);
                    ctx.SaveChanges();
                    ctx.Users.Remove(user);
                    ctx.SaveChanges();
                    ctx.Chats.Remove(chat);
                    ctx.SaveChanges();
                    ctx.MessageTypes.Remove(messageType);
                    ctx.UserRoles.Remove(role);
                    ctx.ChatTypes.Remove(chatType);
                    ctx.Bots.Remove(bot);
                    ctx.SaveChanges();
                    ctx.Messengers.Remove(messenger);

                    ctx.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        //[TestMethod]
        //public void LoadUsersFromJson()
        //{
        //    try
        //    {
        //        var filePath = @"C:\Users\zuev56\Documents\users_backup_20200816_1853.json";
        //
        //        DataImporter.LoadUsersFromJson(filePath, _contextFactory.GetContext());
        //    }
        //    catch (Exception e)
        //    {
        //        throw;
        //    }
        //}

        //[TestMethod]
        //public void LoadMessagesFromJson()
        //{
        //    try
        //    {
        //        var filePath = @"C:\Users\zuev56\Documents\result.json";
        //
        //        //var zlTelegramChatId = -1001364555739;
        //        //DataImporter.LoadMessagesFromJson(filePath, zlTelegramChatId, "ЖК Зима Лето Корпус 6 соседи", "zimaleto96");
        //
        //        var dotNetRuChatId = 9656792576;
        //        DataImporter.LoadMessagesFromJson(filePath, _contextFactory.GetContext(), dotNetRuChatId, "DotNetRuChat", "DotNetRuChat");
        //
        //    }
        //    catch (Exception ex)
        //    {
        //        throw;
        //    }
        //}


    }
}
