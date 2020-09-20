using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Unicode;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;
using Telegram.Bot.Types.Enums;
using Zs.Bot.Model;
using Zs.Common.Extensions;
using TelegramMessageType = Telegram.Bot.Types.Enums.MessageType;

namespace Zs.Bot.Messenger.Telegram
{
    public static class DataImporter
    {
        private static readonly Encoding _utf8Encoder = Encoding.GetEncoding(
            "UTF-8",
            new EncoderReplacementFallback(string.Empty),
            new DecoderExceptionFallback()
        );



        public static void LoadUsersFromJson(string filePath, BotContext context)
        {
            try
            {
                if (!File.Exists(filePath))
                    throw new FileNotFoundException("Не удалось найти файл", filePath);
                var fileContent = File.ReadAllText(filePath);

                var users = new List<User>();
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
                    var insertDate = DateTime.Parse(jItem["InsertDate"].ToString());
                    var jObj = JObject.Parse(jItem.ToString().NormalizeJsonString());
                    jObj.Remove("InsertDate");

                    var fullName = $"{jItem["FirstName"]} {jItem["LastName"]}".ToString().Trim();
                    var userName = string.IsNullOrWhiteSpace(jItem["Username"].ToString()) ? fullName : $"{jItem["Username"]}";

                    users.Add(new User
                    {
                        Name         = string.IsNullOrWhiteSpace(jItem["Username"].ToString()) ? fullName : $"{jItem["Username"]}",
                        FullName     = (string.IsNullOrWhiteSpace(fullName) || userName == fullName) ? null : fullName,
                        UserRoleCode = "USER",
                        IsBot        = isBot,
                        RawData      = jObj.ToString(),
                        RawDataHash  = jObj.ToString().GetMD5Hash(),
                        InsertDate   = insertDate - TimeSpan.FromHours(3),
                        UpdateDate   = DateTime.Now
                    });
                }
                context.Users.AddRange(users);
                int saved = context.SaveChanges();

                // Сдвиг SEQUENCE
                context.Database.ExecuteSqlRaw($"SELECT setval('bot.users_user_id_seq', COALESCE((SELECT MAX(user_id)+1 FROM bot.users), 1), false);");
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        /// <summary>
        /// Load exported from Telegram messages to database
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="telegramChatId"></param>
        public static void LoadMessagesFromJson(string filePath, BotContext context, long telegramChatId, string telegramChatTitle, string telegramChatName)
        {
            try
            {
                if (!File.Exists(filePath))
                    throw new FileNotFoundException("Не удалось найти файл", filePath);
                var fileContent = _utf8Encoder.GetString(_utf8Encoder.GetBytes(File.ReadAllText(filePath)));
                fileContent = new string(fileContent.Where(x => !char.IsSurrogate(x)).ToArray());

                var messageId = context.Messages.Count() + 1;
                int lastSavedId = messageId;

                var dbChatId = context.Chats
                    .FromSqlRaw("SELECT * FROM bot.chats WHERE CAST(raw_data ->> 'Id' AS BIGINT) = {0}", telegramChatId)
                    .FirstOrDefault()?.Id;

                if (dbChatId is null)
                {
                    var jsonChat = ("{\n"
                             +$"  \"Id\": {telegramChatId},\n"
                             + "  \"Type\": 3,\n"
                             +$"  \"Title\": \"{telegramChatTitle}\",\n"
                             + "  \"AllMembersAreAdministrators\": false\n"
                             + "}\n").NormalizeJsonString();
                    var chat = new Chat()
                    {
                        Description = $"{telegramChatTitle}",
                        Name        = $"{telegramChatName}",
                        ChatTypeCode    = "GROUP",
                        RawData         = jsonChat,
                        RawDataHash     = jsonChat.GetMD5Hash(),
                        InsertDate      = DateTime.Parse("2018-04-14T22:49:37"),
                        UpdateDate      = DateTime.Now
                    };
                    context.Chats.Add(chat);
                    context.SaveChanges();
                    dbChatId = chat.Id;
                }

                var chatContent = JObject.Parse(fileContent);
                var jsonMessages = chatContent["messages"]
                    .Where(i => i["type"].ToString() == "message")
                    .OrderBy(i => (int)i["id"]).ToList(); // Сортировка важна для корректного получения ReplyToDbMessageId

                var dbMessagesDict = new ConcurrentDictionary<int, Message>(Environment.ProcessorCount, 80000);
                var Users = context.Users.ToList()
                    .Select(u => new {
                        UserId = u.Id,
                        tgUserId = ((int?)JObject.Parse(u.RawData)
                            .Properties().FirstOrDefault(p => p.Name == "Id")?.Value ?? -10)
                    }).ToList();

                var options = new JsonSerializerOptions
                {
                    IgnoreNullValues = true,
                    WriteIndented = true,
                    Encoder = JavaScriptEncoder.Create(UnicodeRanges.All)
                };

                foreach (var jItem in jsonMessages)
                {
                    if (jItem["from_id"] is { })
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
                        try
                        {
                            if (text.Trim().StartsWith('['))
                            {
                                var clearText = new StringBuilder();
                                var arr = JToken.Parse(text).ToArray();
                                foreach (var item in JToken.Parse(text).ToArray())
                                {
                                    if (item is JObject)
                                        clearText.Append(item["text"]);
                                    else
                                        clearText.Append(((JValue)item).Value.ToString());
                                }
                                text = clearText.ToString();
                            }
                            else
                                text = string.IsNullOrWhiteSpace(text) ? stickerEmoji ?? "" : text;
                        }
                        catch
                        {
                            text = string.IsNullOrWhiteSpace(text) ? stickerEmoji ?? "" : text;
                        }

                        var replyToMsgId    = (int?)jItem["reply_to_message_id"];
                        var forwardedFromId = jItem["forwarded_from"] != null ? (int?)-10 : null;

                        int UserId = Users
                            .FirstOrDefault(u => u.tgUserId == fromId)?.UserId ?? -10;

                        var tgMessageType = mediaType switch
                        {
                            "STK" => TelegramMessageType.Sticker,
                            "VOI" => TelegramMessageType.Voice,
                            "VID" => TelegramMessageType.Video,
                            "AUD" => TelegramMessageType.Audio,
                            "TXT" => TelegramMessageType.Text,
                            _     => TelegramMessageType.Unknown
                        };

                        var jsonMessage = new JObject(
                            new JProperty("MessageId", id),
                            new JProperty("ChatId", telegramChatId),
                            new JProperty("FromId", fromId),
                            new JProperty("Type", (int)tgMessageType),
                            new JProperty("Text", text),
                            new JProperty("Date", date));

                        if (replyToMsgId != null)
                            jsonMessage.Add(new JProperty("ReplyToMessageId", replyToMsgId));

                        if (forwardedFromId != null)
                            jsonMessage.Add(new JProperty("ForwardFromId", forwardedFromId));

                        int? ReplyToDbMessageId = null;
                        if (replyToMsgId != null)
                            ReplyToDbMessageId = dbMessagesDict.ContainsKey((int)replyToMsgId)
                                               ? replyToMsgId
                                               : null;

                        var dbMessage = new Message
                        {
                            Id               = id,
                            ReplyToMessageId = ReplyToDbMessageId,
                            MessengerCode    = "TG",
                            MessageTypeCode  = mediaType,
                            UserId           = UserId,
                            ChatId           = (int)dbChatId,
                            Text             = text?.Length > 100 ? text.Substring(0, 100) : text,
                            IsSucceed        = true,
                            FailsCount       = 0,
                            FailDescription  = null,
                            IsDeleted        = false,
                            RawData          = jsonMessage.ToString(),
                            RawDataHash      = jsonMessage.ToString().GetMD5Hash(),
                            InsertDate       = date - TimeSpan.FromHours(3),
                            UpdateDate       = DateTime.Now
                        };

                        if (!dbMessagesDict.TryAdd(id, dbMessage))
                        { }
                    }
                }

                var dbMessages = dbMessagesDict.OrderBy(i => i.Key).Select(i => i.Value).ToList();
                for (int i = 0; i < dbMessages.Count; i++)
                {
                    for (int j = i + 1; j < dbMessages.Count; j++)
                        if (dbMessages[j].ReplyToMessageId == dbMessages[i].Id)
                            dbMessages[j].ReplyToMessageId = i + 1;

                    dbMessages[i].Id = i + 1;
                }


                // TODO: Упростить сохранение - сделать всё, как в последнем блоке


                var notReplies = dbMessages.Where(m => m.ReplyToMessageId == null).ToList();
                var replies = dbMessages.Where(m => m.ReplyToMessageId != null).ToList();
                var freeReplies = new List<Message>();
                var savedMessageIds = new List<int>(80000);

                try
                {
                    context.Messages.AddRange(notReplies);
                    int saved = context.SaveChanges();
                    savedMessageIds.AddRange(notReplies.Select(m => m.Id));

                    // На сообщения-ответы могут также отвечать. 
                    // Поэтому надо сначала добавить те сообщения, которые являются ответами на сообщения, которых нет в списке replies (eсть в списке notReplies)
                    // а оставшиеся либо по очереди, либо по особому принципу

                    // freeReplies - это сообщения, которые являются ответами на другие, но на которые уже никто не отвечаел

                    freeReplies = replies.Where(m => !replies.Select(p => p.Id).Contains((int)m.ReplyToMessageId)).ToList();
                    context.Messages.AddRange(freeReplies);
                    saved = context.SaveChanges();
                    savedMessageIds.AddRange(freeReplies.Select(m => m.Id));

                    // Здесь в replies остались сообщения, которые являются ответами и на которые также кто-то отвечает
                    // Надо рекурсивно выбирать из списка сообщения, которые отвечают на те, которые уже записаны в БД
                    // и такими пачками по очереди производить сохранение


                    replies = replies.Where(m => !freeReplies.Select(p => p.Id).Contains(m.Id)).ToList();

                    var part = new List<Message>();
                    while (replies.Any())
                    {
                        part.AddRange(replies.Where(m => savedMessageIds.Contains((int)m.ReplyToMessageId)));
                        replies.RemoveAll(m => part.Contains(m));

                        context.Messages.AddRange(part);
                        context.SaveChanges();

                        savedMessageIds.AddRange(part.Select(m => m.Id));
                        part.Clear();
                    }

                    // Сдвиг SEQUENCE
                    context.Database.ExecuteSqlRaw("SELECT setval('bot.messages_message_id_seq', COALESCE((SELECT MAX(message_id)+1 FROM bot.messages), 1), false);");
                }
                catch (Exception ex)
                {
                    throw;
                }
            }
            catch (Exception ex)
            {
                throw;
            }
        }

    }
}
