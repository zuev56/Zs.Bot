using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Unicode;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;
using Telegram.Bot.Types.Enums;
using Zs.Bot.Model.Db;
using Zs.Common.Extensions;

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
                using (var ctx = new ZsBotDbContext())
                {
                    Assert.IsNotNull(ctx.Bots.FirstOrDefault());
                    Assert.IsNotNull(ctx.ChatTypes.FirstOrDefault());
                    Assert.IsNotNull(ctx.Chats.FirstOrDefault());
                    Assert.IsNotNull(ctx.UserRoles.FirstOrDefault());
                    Assert.IsNotNull(ctx.Users.FirstOrDefault());
                    Assert.IsNotNull(ctx.Messengers.FirstOrDefault());
                    Assert.IsNotNull(ctx.MessageTypes.FirstOrDefault());
                    Assert.IsNotNull(ctx.Messages.FirstOrDefault());
                    Assert.IsNotNull(ctx.Sessions.FirstOrDefault());
                    Assert.IsNotNull(ctx.Logs.FirstOrDefault());
                    Assert.IsNotNull(ctx.Commands.FirstOrDefault());
                    Assert.IsNotNull(ctx.Options.FirstOrDefault());
                    //Assert.IsNotNull(ctx.SqlResults.FirstOrDefault()); Не таблица БД
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
                using var ctx = new ZsBotDbContext();

                var bot             = ctx.Bots.FirstOrDefault();
                var chatType        = ctx.ChatTypes.FirstOrDefault();
                var chat            = ctx.Chats.FirstOrDefault();
                var role            = ctx.UserRoles.FirstOrDefault();
                var user            = ctx.Users.FirstOrDefault();
                var messenger       = ctx.Messengers.FirstOrDefault();
                var messageType     = ctx.MessageTypes.FirstOrDefault();
                var message         = ctx.Messages.FirstOrDefault();
                var session         = ctx.Sessions.FirstOrDefault();
                var log             = ctx.Logs.FirstOrDefault();
                var command         = ctx.Commands.FirstOrDefault();
                var option          = ctx.Options.FirstOrDefault();

                var botId           = bot.BotId;
                var chatTypeCode    = chatType.ChatTypeCode;
                var chatId          = chat.ChatId;
                var roleCode        = role.UserRoleCode;
                var userId          = user.UserId;
                var messengerCode   = messenger.MessengerCode;
                var messageTypeCode = messageType.MessageTypeCode;
                var messageId       = message.MessageId;
                var sessionId       = session.SessionId;
                var logId           = log.LogId;
                var commandName     = command.CommandName;
                var optionName      = option.OptionName;

                var newUpdateDate = DateTime.Now;
                bot.UpdateDate             = newUpdateDate;
                chatType.UpdateDate        = newUpdateDate;
                chat.UpdateDate            = newUpdateDate;
                role.UpdateDate            = newUpdateDate;
                user.UpdateDate            = newUpdateDate;
                messenger.UpdateDate       = newUpdateDate;
                messageType.UpdateDate     = newUpdateDate;
                message.UpdateDate         = newUpdateDate;
                session.UpdateDate         = newUpdateDate;
                log.LogMessage             = $"{newUpdateDate}";
                command.UpdateDate         = newUpdateDate;
                option.UpdateDate          = newUpdateDate;

                ctx.SaveChanges();

                Assert.IsTrue(newUpdateDate == ctx.Bots.First(b => b.BotId == botId).UpdateDate);
                Assert.IsTrue(newUpdateDate == ctx.ChatTypes.First(t => t.ChatTypeCode == chatTypeCode).UpdateDate);
                Assert.IsTrue(newUpdateDate == ctx.Chats.First(c => c.ChatId == chatId).UpdateDate);
                Assert.IsTrue(newUpdateDate == ctx.UserRoles.First(r => r.UserRoleCode == roleCode).UpdateDate);
                Assert.IsTrue(newUpdateDate == ctx.Users.First(u => u.UserId == userId).UpdateDate);
                Assert.IsTrue(newUpdateDate == ctx.Messengers.First(m => m.MessengerCode == messengerCode).UpdateDate);
                Assert.IsTrue(newUpdateDate == ctx.MessageTypes.First(t => t.MessageTypeCode == messageTypeCode).UpdateDate);
                Assert.IsTrue(newUpdateDate == ctx.Messages.First(m => m.MessageId == messageId).UpdateDate);
                Assert.IsTrue(newUpdateDate == ctx.Sessions.First(s => s.SessionId == sessionId).UpdateDate);
                Assert.IsTrue($"{newUpdateDate}" == ctx.Logs.First(l => l.LogId == logId).LogMessage);
                Assert.IsTrue(newUpdateDate == ctx.Commands.First(c => c.CommandName == commandName).UpdateDate);
                Assert.IsTrue(newUpdateDate == ctx.Options.First(o => o.OptionName == optionName).UpdateDate);
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

                var messenger       = new DbMessengerInfo   { MessengerCode = "U1", MessengerName = "UnitTest1" };
                var bot             = new DbBot             { BotId = -2, MessengerCode = "U1", BotName = "UnitTest1", BotToken = "UnitTest1" };
                var chatType        = new DbChatType        { ChatTypeCode = "UNITTEST1", ChatTypeName = "UnitTest1"};
                var chat            = new DbChat            { ChatId = -2, ChatTypeCode = "UNITTEST1", ChatName = "UnitTest1", RawData = testJsonValue, RawDataHash = testJsonValue.GetMD5Hash()};
                var role            = new DbUserRole        { UserRoleCode = "UNITTEST1", UserRoleName = "UnitTest1", UserRolePermissions = "[ \"UnitTest1\" ]" };
                var user            = new DbUser            { UserId = -2, UserName = "UnitTest1", UserRoleCode = "UNITTEST1", UserIsBot = false, RawData = testJsonValue, RawDataHash = testJsonValue.GetMD5Hash() };
                var messageType     = new DbMessageType     { MessageTypeCode = "U1", MessageTypeName = "UnitTest1" };
                var message         = new DbMessage         { MessageId = -2, MessengerCode = "U1", MessageTypeCode = "U1", ChatId = -2, UserId = -2, RawData = testJsonValue };
                var session         = new DbSession         { SessionId = -2, SessionCurrentState = testJsonValue, ChatId = -2, SessionIsLoggedIn = false };
                var log             = new DbLog             { LogId = -2, LogType = "WARNING", LogMessage = "UnitTest1" };
                var command         = new DbCommand         { CommandName = "/unittest1", CommandScript = "UnitTest1", CommandGroup = "UnitTest1" };
                var option          = new DbOption          { OptionName = "UnitTest1", OptionGroup = "UnitTest1" };

                using (var ctx = new ZsBotDbContext())
                {
                    if (!ctx.Messengers.Any(m => m.MessengerCode == messenger.MessengerCode))
                        ctx.Messengers.Add(messenger);

                    if (!ctx.ChatTypes.Any(t => t.ChatTypeCode == chatType.ChatTypeCode))
                        ctx.ChatTypes.Add(chatType);

                    if (!ctx.UserRoles.Any(r => r.UserRoleCode == role.UserRoleCode))
                        ctx.UserRoles.Add(role);

                    if (!ctx.MessageTypes.Any(t => t.MessageTypeCode == messageType.MessageTypeCode))
                        ctx.MessageTypes.Add(messageType);

                    if (ctx.ChangeTracker.HasChanges())
                        ctx.SaveChanges();

                    if (!ctx.Bots.Any(b => b.BotId == bot.BotId))
                    {
                        ctx.Bots.Add(bot);
                        ctx.SaveChanges();
                    }

                    if (!ctx.Chats.Any(c => c.ChatId == chat.ChatId))
                    {
                        ctx.Chats.Add(chat);
                        ctx.SaveChanges();
                    }

                    if (!ctx.Users.Any(u => u.UserId == user.UserId))
                    {
                        ctx.Users.Add(user);
                        ctx.SaveChanges();
                    }

                    if (!ctx.Messages.Any(m => m.MessageId == message.MessageId))
                    {
                        ctx.Messages.Add(message);
                        ctx.SaveChanges();
                    }

                    if (!ctx.Sessions.Any(s => s.SessionId == session.SessionId))
                        ctx.Sessions.Add(session);

                    if (!ctx.Logs.Any(l => l.LogId == log.LogId))
                        ctx.Logs.Add(log);

                    if (!ctx.Commands.Any(c => c.CommandName == command.CommandName))
                        ctx.Commands.Add(command);

                    if (!ctx.Options.Any(o => o.OptionName == option.OptionName))
                        ctx.Options.Add(option);

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

                var messenger       = new DbMessengerInfo   { MessengerCode = "U0", MessengerName = "UnitTest0" };
                var bot             = new DbBot             { BotId = -1, MessengerCode = "U0", BotName = "UnitTest0", BotToken = "UnitTest0" };
                var chatType        = new DbChatType        { ChatTypeCode = "UNITTEST0", ChatTypeName = "UnitTest0"};
                var chat            = new DbChat            { ChatId = -1, ChatTypeCode = "UNITTEST0", ChatName = "UnitTest0", RawData = testJsonValue, RawDataHash = testJsonValue.GetMD5Hash()};
                var role            = new DbUserRole        { UserRoleCode = "UNITTEST0", UserRoleName = "UnitTest0", UserRolePermissions = "[ \"UnitTest0\" ]" };
                var user            = new DbUser            { UserId = -1, UserName = "UnitTest0", UserRoleCode = "UNITTEST0", UserIsBot = false, RawData = testJsonValue, RawDataHash = testJsonValue.GetMD5Hash() };
                var messageType     = new DbMessageType     { MessageTypeCode = "U0", MessageTypeName = "UnitTest0" };
                var message         = new DbMessage         { MessageId = -1, MessengerCode = "U0", MessageTypeCode = "U0", ChatId = -1, UserId = -1, RawData = testJsonValue };
                var session         = new DbSession         { SessionId = -1, SessionCurrentState = testJsonValue, ChatId = -1, SessionIsLoggedIn = false };
                var log             = new DbLog             { LogId = -1, LogType = "WARNING", LogMessage = "UnitTest0" };
                var command         = new DbCommand         { CommandName = "/unittest0", CommandScript = "UnitTest0", CommandGroup = "UnitTest0" };
                var option          = new DbOption          { OptionName = "UnitTest0", OptionGroup = "UnitTest0" };

                //Сохранение
                using (var ctx = new ZsBotDbContext())
                {
                    if (!ctx.Messengers.Any(m => m.MessengerCode == messenger.MessengerCode))
                        ctx.Messengers.Add(messenger);

                    if (!ctx.ChatTypes.Any(t => t.ChatTypeCode == chatType.ChatTypeCode))
                        ctx.ChatTypes.Add(chatType);

                    if (!ctx.UserRoles.Any(r => r.UserRoleCode == role.UserRoleCode))
                        ctx.UserRoles.Add(role);

                    if (!ctx.MessageTypes.Any(t => t.MessageTypeCode == messageType.MessageTypeCode))
                        ctx.MessageTypes.Add(messageType);

                    if (ctx.ChangeTracker.HasChanges())
                        ctx.SaveChanges();

                    if (!ctx.Bots.Any(b => b.BotId == bot.BotId))
                    {
                        ctx.Bots.Add(bot); 
                        ctx.SaveChanges();
                    }

                    if (!ctx.Chats.Any(c => c.ChatId == chat.ChatId))
                    {
                        ctx.Chats.Add(chat);
                        ctx.SaveChanges();
                    }

                    if (!ctx.Users.Any(u => u.UserId == user.UserId))
                    {
                        ctx.Users.Add(user); 
                        ctx.SaveChanges();
                    }

                    if (!ctx.Messages.Any(m => m.MessageId == message.MessageId))
                    {
                        ctx.Messages.Add(message); 
                        ctx.SaveChanges();
                    }

                    if (!ctx.Sessions.Any(s => s.SessionId == session.SessionId))
                        ctx.Sessions.Add(session);

                    if (!ctx.Logs.Any(l => l.LogId == log.LogId))
                        ctx.Logs.Add(log);

                    if (!ctx.Commands.Any(c => c.CommandName == command.CommandName))
                        ctx.Commands.Add(command);

                    if (!ctx.Options.Any(o => o.OptionName == option.OptionName))
                        ctx.Options.Add(option);

                    if (ctx.ChangeTracker.HasChanges())
                        ctx.SaveChanges();
                }

                // Удаление из БД
                using (var ctx = new ZsBotDbContext())
                {
                    ctx.Logs.Remove(log);
                    ctx.Commands.Remove(command);
                    ctx.Options.Remove(option);
                    ctx.Sessions.Remove(session);
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
        public void LoadUsersFromJson()
        {
            try
            {
                var filePath = @"C:\Users\zuev56\Documents\users_backup_20200613_1332.json";
                if (!File.Exists(filePath))
                    throw new FileNotFoundException("Не удалось найти файл", filePath);
                var fileContent = File.ReadAllText(filePath);

                var users = new List<DbUser>();
                using var ctx = new ZsBotDbContext();
                foreach (var jItem in JArray.Parse(fileContent))
                {
                    var firstName = string.IsNullOrWhiteSpace(jItem["FirstName"]?.ToString())
                        ? null 
                        : $"\"{jItem["FirstName"]}\"";
                    var lastName  = string.IsNullOrWhiteSpace(jItem["LastName"]?.ToString())
                        ? null 
                        : $"\"{jItem["LastName"]}\"";
                    var isBot  = string.IsNullOrWhiteSpace(jItem["IsBot"]?.ToString())
                        ? false
                        : bool.Parse(jItem["IsBot"].ToString());
                    var insertDate = DateTime.Parse(jItem["insertdate"].ToString());
                    var jObj = JObject.Parse(jItem.ToString());
                    jObj.Remove("insertdate");

                    var fullName = $"{jItem["FirstName"]} {jItem["LastName"]}".ToString().Trim();
                    var userName = string.IsNullOrWhiteSpace(jItem["UserName"].ToString()) ? fullName : $"{jItem["UserName"]}";
                    
                    users.Add(new DbUser
                    {
                        UserName     = string.IsNullOrWhiteSpace(jItem["UserName"].ToString()) ? fullName : $"{jItem["UserName"]}",
                        UserFullName = (string.IsNullOrWhiteSpace(fullName) || userName == fullName) ? null : fullName,
                        UserRoleCode = "USER",
                        UserIsBot    = isBot,
                        RawData      = jObj.ToString(),
                        RawDataHash  = jObj.ToString().GetMD5Hash(),
                        InsertDate   = insertDate,
                        UpdateDate   = DateTime.Now
                    });
                }
                ctx.Users.AddRange(users);
                int saved = ctx.SaveChanges();
            }
            catch (Exception e)
            {
                throw;
            }
        }
        
        //[TestMethod]
        public void LoadMessagesFromJson()
        {
            try
            {
                var zlTelegramChatId = -1001364555739;
                var filePath = @"C:\Users\zuev56\Documents\ExportedMessages.json";
                if (!File.Exists(filePath))
                    throw new FileNotFoundException("Не удалось найти файл", filePath);
                var fileContent = Utf8Encoder.GetString(Utf8Encoder.GetBytes(File.ReadAllText(filePath)));
                fileContent = new string(fileContent.Where(x => !char.IsSurrogate(x)).ToArray());

                using var ctx = new ZsBotDbContext();
                var messageId = ctx.Messages.Count() + 1;
                int lastSavedId = messageId;

                var dbChatId = ctx.Chats
                    .FromSqlRaw("SELECT * FROM bot.chats WHERE CAST(raw_data ->> 'Id' AS BIGINT) = {0}", zlTelegramChatId)
                    .FirstOrDefault()?.ChatId;

                if (dbChatId is null)
                {
                    var jsonChat = "{\n"
                             +$"  \"Id\": {zlTelegramChatId},\n"
                             + "  \"Type\": 3,\n"
                             + "  \"Title\": \"ЖК Зима Лето Корпус 6 соседи\",\n"
                             + "  \"AllMembersAreAdministrators\": false\n"
                             + "}\n";
                    var chat = new DbChat()
                    {
                        ChatDescription = "ЖК Зима Лето Корпус 6 соседи",
                        ChatName        = "zimaleto96",
                        ChatTypeCode    = "GROUP",
                        RawData         = jsonChat,
                        RawDataHash     = jsonChat.GetMD5Hash(),
                        InsertDate      = DateTime.Parse("2018-04-14T22:49:37"),
                        UpdateDate      = DateTime.Now
                    };
                    ctx.Chats.Add(chat);
                    ctx.SaveChanges();
                    dbChatId = chat.ChatId;
                }

                var chatContent = JObject.Parse(fileContent);
                var jsonMessages = chatContent["messages"]
                    .Where(i => i["type"].ToString() == "message")
                    .OrderBy(i => (int)i["id"]).ToList(); // Сортировка важна для корректного получения ReplyToDbMessageId

                var dbMessages = new List<DbMessage>();
                var dbUsers = ctx.Users.ToList()
                    .Select(u => new {
                        dbUserId = u.UserId,
                        tgUserId = ((int?)JObject.Parse(u.RawData)
                            .Properties().FirstOrDefault(p => p.Name == "Id")?.Value ?? -10)
                    }).ToList();

                var options = new JsonSerializerOptions
                {
                    IgnoreNullValues = true,
                    WriteIndented = true,
                    Encoder = JavaScriptEncoder.Create(UnicodeRanges.All)
                };
                var locker = new object();

                foreach (var jItem in jsonMessages)
                {
                    if (jItem["from_id"] is {})
                    {

                        int id              = (int)jItem["id"];
                        var date            = DateTime.Parse(jItem["date"].ToString());
                        var edited          = jItem["edited"] != null ? (DateTime?)DateTime.Parse(jItem["edited"].ToString()) : null;
                        var fromId          = (int)jItem["from_id"];
                        var mediaType       = jItem["media_type"] != null
                            ? jItem["media_type"].ToString() switch
                              {
                                  "sticker"       => "STK",
                                  "voice_message" => "VOI",
                                  "video_file"    => "VID",
                                  "audio_file"    => "AUD",
                                                _ => "OTH"
                              }
                            : "TXT";
                        var stickerEmoji    = jItem["sticker_emoji"]?.ToString();
                        var text            = jItem["text"]?.ToString();
                        try { JToken.Parse(text); } 
                        catch { 
                            //text = $"\"{text}\"";
                            text = string.IsNullOrWhiteSpace(text) ? stickerEmoji ?? "" : text;
                        }
                            
                        var replyToMsgId    = (int?)jItem["reply_to_message_id"];
                        var forwardedFromId = jItem["forwarded_from"] != null ? (int?)-10 : null;

                        int dbUserId = dbUsers
                            .FirstOrDefault(u => u.tgUserId == fromId)?.dbUserId ?? -10;

                        var tgMessageType = mediaType switch
                        {
                            "STK" => MessageType.Sticker,
                            "VOI" => MessageType.Voice,
                            "VID" => MessageType.Video,
                            "AUD" => MessageType.Audio,
                            "TXT" => MessageType.Text,
                                _ => MessageType.Unknown
                        };

                        var jsonMessage = new JObject(
                            new JProperty("MessageId", id),
                            new JProperty("ChatId", zlTelegramChatId),
                            new JProperty("FromId", fromId),
                            new JProperty("Type", (int)tgMessageType),
                            new JProperty("Text", text),
                            new JProperty("Date", date));
                        
                        if (replyToMsgId != null)
                            jsonMessage.Add(new JProperty("ReplyToMessageId", replyToMsgId));
                        
                        if (forwardedFromId != null)
                            jsonMessage.Add(new JProperty("ForwardFromId", forwardedFromId));

                        //text = text.Trim('\"');
                        int? ReplyToDbMessageId = null;

                        try
                        {
                            if (replyToMsgId != null)
                            {
                               // Может быть NULL, если сообщение, на которое отвечаем, было удалено
                               ReplyToDbMessageId = dbMessages?
                                   .FirstOrDefault(m => (int)JObject.Parse(m.RawData).Properties()
                                                        ?.First(p => string.Equals(p.Name, "MessageId", StringComparison.InvariantCultureIgnoreCase))
                                                        ?.Value == replyToMsgId)?.MessageId;
                            }
                        }
                        catch (Exception ex) { }

                        dbMessages.Add(new DbMessage
                        {
                            MessageId        = messageId++,
                            ReplyToMessageId = ReplyToDbMessageId,
                            MessengerCode    = "TG",
                            MessageTypeCode  = mediaType,
                            UserId           = dbUserId,
                            ChatId           = (int)dbChatId,
                            MessageText      = text?.Length > 100 ? text.Substring(0, 100) : text,
                            IsSucceed        = true,
                            FailsCount       = 0,
                            FailDescription  = null,
                            IsDeleted        = false,
                            RawData          = jsonMessage.ToString(),
                            InsertDate       = date,
                            UpdateDate       = DateTime.Now
                        });
                    }
                }
                var notReplies = dbMessages.Where(m => m.ReplyToMessageId == null).ToList();
                var replies = dbMessages.Where(m => m.ReplyToMessageId != null).ToList();

                ctx.Messages.AddRange(notReplies);
                int saved = ctx.SaveChanges();

                // На сообщения-ответы могут также отвечать. 
                // Поэтому надо сначала добавить те сообщения, которые являются ответами на сообщения, которых нет в списке replies (eсть в списке notReplies)
                // а оставшиеся либо по очереди, либо по особому принципу

                var freeReplies = replies.Where(m => !replies.Select(p => p.MessageId).Contains((int)m.ReplyToMessageId)).ToList();
                replies = replies.Where(m => !freeReplies.Select(p => p.MessageId).Contains(m.MessageId)).ToList();
                ctx.Messages.AddRange(freeReplies);
                saved = ctx.SaveChanges();

                foreach (var m in replies)
                {
                    ctx.Messages.Add(m);
                    ctx.SaveChanges();
                }

            }
            catch (Exception e)
            {
                throw;
            }
        }

        private static class JSonMessage
        {
            public static Dictionary<string, object> Properties
                => new Dictionary<string, object>()
                {
                    {"MessageId",        null },
                    {"ChatId",           null },
                    {"FromId",           null },
                    {"Type",             null },
                    {"Text",             null },
                    {"Date",             null },
                    {"ReplyToMessageId", null },
                    {"ForwardFromId",    null },
                };
        }

        public static TSource Value<TSource>(object jsonbProperty, string jsonbPropertyName)
        {
            throw new NotSupportedException();
        }

    }
}
