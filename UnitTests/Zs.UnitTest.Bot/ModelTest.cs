using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Zs.Bot.Data;
using Zs.Bot.Data.Models;
using Zs.Common.Extensions;

namespace Zs.UnitTest.Bot
{
    [TestClass]
    public class ModelTest : DataBaseClient
    {
        [TestMethod]
        public void DbModel_SelectTest()
        {
            try
            {
                using (var ctx = _contextFactory.GetContext())
                {
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

                var chatType    = ctx.ChatTypes.FirstOrDefault();
                var chat        = ctx.Chats.FirstOrDefault();
                var role        = ctx.UserRoles.FirstOrDefault();
                var user        = ctx.Users.FirstOrDefault();
                var messenger   = ctx.Messengers.FirstOrDefault();
                var messageType = ctx.MessageTypes.FirstOrDefault();
                var message     = ctx.Messages.FirstOrDefault();
                var log         = ctx.Logs.FirstOrDefault();
                var command     = ctx.Commands.FirstOrDefault();

                var chatTypeCode    = chatType.Id;
                var chatId          = chat.Id;
                var roleCode        = role.Id;
                var userId          = user.Id;
                var messengerCode   = messenger.Id;
                var messageTypeCode = messageType.Id;
                var messageId       = message.Id;
                var logId           = log.Id;
                var commandName     = command.Id;

                var newUpdateDate      = DateTime.Now;
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

                Assert.IsTrue(newUpdateDate == ctx.ChatTypes.First(t => t.Id == chatTypeCode).UpdateDate);
                Assert.IsTrue(newUpdateDate == ctx.Chats.First(c => c.Id == chatId).UpdateDate);
                Assert.IsTrue(newUpdateDate == ctx.UserRoles.First(r => r.Id == roleCode).UpdateDate);
                Assert.IsTrue(newUpdateDate == ctx.Users.First(u => u.Id == userId).UpdateDate);
                Assert.IsTrue(newUpdateDate == ctx.Messengers.First(m => m.Id == messengerCode).UpdateDate);
                Assert.IsTrue(newUpdateDate == ctx.MessageTypes.First(t => t.Id == messageTypeCode).UpdateDate);
                Assert.IsTrue(newUpdateDate == ctx.Messages.First(m => m.Id == messageId).UpdateDate);
                Assert.IsTrue($"{newUpdateDate}" == ctx.Logs.First(l => l.Id == logId).Message);
                Assert.IsTrue(newUpdateDate == ctx.Commands.First(c => c.Id == commandName).UpdateDate);
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

                var messenger       = new MessengerInfo   {Id = "U1", Name = "UnitTest1" };
                var chatType        = new ChatType        {Id = "UNITTEST1", Name = "UnitTest1"};
                var chat            = new Chat            { Id = -2, ChatTypeId = "UNITTEST1", Name = "UnitTest1", RawData = testJsonValue, RawDataHash = testJsonValue.GetMD5Hash()};
                var role            = new UserRole        {Id = "UNITTEST1", Name = "UnitTest1", Permissions = "[ \"UnitTest1\" ]" };
                var user            = new User            { Id = -2, Name = "UnitTest1", UserRoleCode = "UNITTEST1", IsBot = false, RawData = testJsonValue, RawDataHash = testJsonValue.GetMD5Hash() };
                var messageType     = new MessageType     {Id = "U1", Name = "UnitTest1" };
                var message         = new Message         { Id = -2, MessengerId = "U1", MessageTypeCode = "U1", ChatId = -2, UserId = -2, RawData = testJsonValue, RawDataHash = testJsonValue.GetMD5Hash() };
                var log             = new Log             { Id = -2, Type = "WARNING", Message = "UnitTest1" };
                var command         = new Command         { Name = "/unittest1", Script = "UnitTest1", Group = "UnitTest1" };

                using (var ctx = _contextFactory.GetContext())
                {
                    if (!ctx.Messengers.Any(m => m.Id == messenger.Id))
                        ctx.Messengers.Add(messenger);

                    if (!ctx.ChatTypes.Any(t => t.Id == chatType.Id))
                        ctx.ChatTypes.Add(chatType);

                    if (!ctx.UserRoles.Any(r => r.Id == role.Id))
                        ctx.UserRoles.Add(role);

                    if (!ctx.MessageTypes.Any(t => t.Id == messageType.Id))
                        ctx.MessageTypes.Add(messageType);

                    if (ctx.ChangeTracker.HasChanges())
                        ctx.SaveChanges();

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

                var messenger       = new MessengerInfo   {Id = "U0", Name = "UnitTest0" };
                var chatType        = new ChatType        {Id = "UNITTEST0", Name = "UnitTest0"};
                var chat            = new Chat            { Id = -1, ChatTypeCode = "UNITTEST0", Name = "UnitTest0", RawData = testJsonValue, RawDataHash = testJsonValue.GetMD5Hash()};
                var role            = new UserRole        {Id = "UNITTEST0", Name = "UnitTest0", Permissions = "[ \"UnitTest0\" ]" };
                var user            = new User            { Id = -1, Name = "UnitTest0", UserRoleCode = "UNITTEST0", IsBot = false, RawData = testJsonValue, RawDataHash = testJsonValue.GetMD5Hash() };
                var messageType     = new MessageType     {Id = "U0", Name = "UnitTest0" };
                var message         = new Message         { Id = -1, MessengerCode = "U0", MessageTypeCode = "U0", ChatId = -1, UserId = -1, RawData = testJsonValue, RawDataHash = testJsonValue.GetMD5Hash() };
                var log             = new Log             { Id = -1, Type = "WARNING", Message = "UnitTest0" };
                var command         = new Command         { Name = "/unittest0", Script = "UnitTest0", Group = "UnitTest0" };

                using (var ctx = _contextFactory.GetContext())
                {
                    if (!ctx.Messengers.Any(m => m.Id == messenger.Id))
                        ctx.Messengers.Add(messenger);

                    if (!ctx.ChatTypes.Any(t => t.Id == chatType.Id))
                        ctx.ChatTypes.Add(chatType);

                    if (!ctx.UserRoles.Any(r => r.Id == role.Id))
                        ctx.UserRoles.Add(role);

                    if (!ctx.MessageTypes.Any(t => t.Id == messageType.Id))
                        ctx.MessageTypes.Add(messageType);

                    if (ctx.ChangeTracker.HasChanges())
                        ctx.SaveChanges();

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
