using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text.Json;
using Newtonsoft.Json.Linq;
using Zs.Bot.Model.Abstractions;
using Zs.Common.Enums;
using Zs.Common.Exceptions;
using Zs.Common.Extensions;

namespace Zs.Bot.Model
{
    public static class ModelExtensions
    {
        public static bool SaveToDb(this IChat chat, BotContext context)
        {

            if (chat == null)
                throw new ArgumentNullException(nameof(chat));

            if (context == null)
                throw new ArgumentNullException(nameof(context));

            if (!context.Chats.Any(c => c.RawDataHash == chat.RawDataHash))
                context.Chats.Add((Chat)chat);

            return context.SaveChanges() == 1;
        }

        public static bool SaveToDb(this IMessage message, BotContext context)
        {
            if (message == null)
                throw new ArgumentNullException(nameof(message));

            if (context == null)
                throw new ArgumentNullException(nameof(context));

            var cpMessage = message.DeepCopy();
            if (cpMessage.Text?.Length > 100)
                cpMessage.Text = cpMessage.Text.Substring(0, 100);

            if (cpMessage.Id == default)
                context.Messages.Add((Message)cpMessage);
            else
            {
                var oldMessage = context.Messages.FirstOrDefault(m => m.Id == cpMessage.Id);

                if (oldMessage is null)
                    context.Messages.Add((Message)cpMessage);
                else
                    oldMessage.ParseFrom(cpMessage);
            }

            var isSaved = context.SaveChanges() == 1;
            message.Id = cpMessage.Id;

            return isSaved;
        }

        public static bool SaveToDb(this IUser user, BotContext context)
        {
            if (user == null)
                throw new ArgumentNullException(nameof(user));

            if (context == null)
                throw new ArgumentNullException(nameof(context));

            if (!context.Users.Any(u => u.RawDataHash == user.RawDataHash))
                context.Users.Add((User)user);

            return context.SaveChanges() == 1;
        }

        public static bool UpdateRawData(this IChat chat, BotContext context)
        {
            if (chat is null)
                throw new ArgumentNullException(nameof(chat));

            if (context == null)
                throw new ArgumentNullException(nameof(context));

            var originalChat = context.Chats.FirstOrDefault(u => u.Id == chat.Id);

            if (originalChat is null)
                throw new ArgumentOutOfRangeException(nameof(chat.Id));

            if (originalChat.RawDataHash == chat.RawDataHash)
                return false;
            else
            {
                if (originalChat.RawDataHistory is null)
                {
                    originalChat.RawDataHistory = $"[{originalChat.RawData}]".NormalizeJsonString();
                }
                else
                {
                    var rdHistory = JArray.Parse(originalChat.RawDataHistory);
                    rdHistory.Add(originalChat.RawData);

                    originalChat.RawDataHistory = rdHistory.ToString().NormalizeJsonString();
                }

                originalChat.Name = chat.Name;
                originalChat.Description = chat.Description;
                originalChat.ChatTypeCode = chat.ChatTypeCode;
                originalChat.RawData = chat.RawData;
                originalChat.RawDataHash = originalChat.RawData.GetMD5Hash();

                return context.SaveChanges() == 1;
            }
        }

        public static bool UpdateRawData(this IMessage message, BotContext context)
        {
            if (message is null)
                throw new ArgumentNullException(nameof(message));

            if (context == null)
                throw new ArgumentNullException(nameof(context));

            var originalMessage = context.Messages.FirstOrDefault(m => m.Id == message.Id);

            if (originalMessage is null)
                throw new ArgumentOutOfRangeException(nameof(message.Id));

            if (originalMessage.RawDataHash == message.RawDataHash)
                return false;
            else
            {
                if (originalMessage.RawDataHistory is null)
                {
                    originalMessage.RawDataHistory = $"[{originalMessage.RawData}]".NormalizeJsonString();
                }
                else
                {
                    var rdHistory = JArray.Parse(originalMessage.RawDataHistory);
                    rdHistory.Add(originalMessage.RawData);

                    originalMessage.RawDataHistory = rdHistory.ToString().NormalizeJsonString();
                }

                originalMessage.Text = message.Text?.Length > 100
                                     ? message.Text.Substring(0, 100)
                                     : message.Text;
                originalMessage.MessageTypeCode = message.MessageTypeCode;
                originalMessage.ReplyToMessageId = message.ReplyToMessageId;
                originalMessage.RawData = message.RawData;
                originalMessage.RawDataHash = originalMessage.RawData.GetMD5Hash();

                return context.SaveChanges() == 1;
            }
        }

        public static bool UpdateRawData(this IUser user, BotContext context)
        {
            if (user is null)
                throw new ArgumentNullException(nameof(user));

            if (context is null)
                throw new ArgumentNullException(nameof(context));

            var originalUser = context.Users.FirstOrDefault(u => u.Id == user.Id);

            if (originalUser is null)
                throw new ArgumentOutOfRangeException(nameof(user.Id));

            if (originalUser.RawDataHash == user.RawDataHash)
                return false;
            else
            {
                if (originalUser.RawDataHistory is null)
                {
                    originalUser.RawDataHistory = $"[{originalUser.RawData}]".NormalizeJsonString();
                }
                else
                {
                    var rdHistory = JArray.Parse(originalUser.RawDataHistory);
                    rdHistory.Add(originalUser.RawData);

                    originalUser.RawDataHistory = rdHistory.ToString().NormalizeJsonString();
                }

                originalUser.Name = user.Name;
                originalUser.FullName = user.FullName;
                originalUser.RawData = user.RawData;
                originalUser.RawDataHash = originalUser.RawData.GetMD5Hash();

                return context.SaveChanges() == 1;
            }
        }

        public static int GetActualId(this IChat chat, BotContext context)
        {
            if (chat == null)
                throw new ArgumentNullException(nameof(chat));

            if (context == null)
                throw new ArgumentNullException(nameof(context));

            var dbChat = context.Chats.FirstOrDefault(c => c.RawDataHash == chat.RawDataHash);

            return dbChat?.Id ?? throw new ItemNotFoundException(chat);
        }

        public static int GetActualId(this IUser user, BotContext context)
        {
            if (user == null)
                throw new ArgumentNullException(nameof(user));

            var dbUser = context.Users
                .FirstOrDefault(u => u.RawDataHash == user.RawDataHash);

            return dbUser?.Id ?? throw new ItemNotFoundException(user);
        }

        public static Chat GetChat(int chatId, BotContext context)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));

            var dbChat = context.Chats.FirstOrDefault(c => c.Id == chatId);

            return dbChat ?? throw new ItemNotFoundException();
        }

        public static void ParseFrom(this IMessage message, IMessage sourceMessage)
        {
            message.Id = sourceMessage.Id;
            message.ReplyToMessageId = sourceMessage.ReplyToMessageId;
            message.MessengerCode = sourceMessage.MessengerCode;
            message.MessageTypeCode = sourceMessage.MessageTypeCode;
            message.UserId = sourceMessage.UserId;
            message.ChatId = sourceMessage.ChatId;
            message.Text = sourceMessage.Text;
            message.RawData = sourceMessage.RawData;
            message.IsSucceed = sourceMessage.IsSucceed;
            message.FailsCount = sourceMessage.FailsCount;
            message.FailDescription = sourceMessage.FailDescription;
            message.IsDeleted = sourceMessage.IsDeleted;
            message.InsertDate = sourceMessage.InsertDate;
            message.UpdateDate = DateTime.Now;
        }

        public static IMessage DeepCopy(this IMessage source)
        {
            return new Message
            {
                Id               = source.Id,
                ReplyToMessageId = source.ReplyToMessageId,
                MessengerCode    = source.MessengerCode,
                MessageTypeCode  = source.MessageTypeCode,
                UserId           = source.UserId,
                ChatId           = source.ChatId,
                Text             = source.Text,
                RawData          = source.RawData,
                RawDataHash      = source.RawDataHash,
                RawDataHistory   = source.RawDataHistory,
                IsSucceed        = source.IsSucceed,
                FailsCount       = source.FailsCount,
                FailDescription  = source.FailDescription,
                IsDeleted        = source.IsDeleted,
                UpdateDate       = source.UpdateDate,
                InsertDate       = source.InsertDate,
            };
        }

    }

    public partial class DbUserRoleExtensions
    {
        public static string[] GetPermissionsArray(string userRoleCode, BotContext context)
        {
            if (userRoleCode == null)
                throw new ArgumentNullException(nameof(userRoleCode));

            if (context == null)
                throw new ArgumentNullException(nameof(context));

            var permissions = context.UserRoles.FirstOrDefault(r => r.Code == userRoleCode)?.Permissions;

            return JsonSerializer.Deserialize<string[]>(permissions);
        }
    }

    public static class DbLogExtensions
    {
        private static int _logMessageMaxLength = -1;

        public static bool SaveToDb(LogType type, string message, BotContext context, string initiator = null, string data = null)
        {
            try
            {
                if (_logMessageMaxLength == -1)
                {
                    var attribute = (StringLengthAttribute)typeof(Log).GetProperty(nameof(Log.Message)).GetCustomAttributes(true)
                        .FirstOrDefault(a => a is StringLengthAttribute);
                    _logMessageMaxLength = attribute?.MaximumLength ?? 100;
                }

                if (message.Length > _logMessageMaxLength)
                    message = message.Substring(0, _logMessageMaxLength - 3) + "...";

                context.Logs.Add(new Log
                {
                    Type = type.ToString(),
                    Message = message,
                    Initiator = initiator,
                    Data = data,
                    InsertDate = DateTime.Now
                });
                return context.SaveChanges() == 1;
            }
            catch
            {
                return false;
            }
        }
    }
}
