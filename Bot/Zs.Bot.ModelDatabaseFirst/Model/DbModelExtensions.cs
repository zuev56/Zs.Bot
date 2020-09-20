using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text.Json;
using Newtonsoft.Json.Linq;
using Zs.Common.Enums;
using Zs.Common.Exceptions;
using Zs.Common.Extensions;

namespace Zs.Bot.Model.Db
{
    public static class DbModelExtensions
    {
        public static bool SaveToDb(this IChat chat, ZsBotDbContext context)
        {

            if (chat == null)
                throw new ArgumentNullException(nameof(chat));

            if (context == null)
                throw new ArgumentNullException(nameof(context));

            if (!context.Chats.Any(c => c.RawDataHash == chat.RawDataHash))
                context.Chats.Add((DbChat)chat);

            return context.SaveChanges() == 1;
        }

        public static bool SaveToDb(this IMessage message, ZsBotDbContext context)
        {
            if (message == null)
                throw new ArgumentNullException(nameof(message));

            if (context == null)
                throw new ArgumentNullException(nameof(context));

            var cpMessage = message.DeepCopy();
            if (cpMessage.MessageText?.Length > 100)
                cpMessage.MessageText = cpMessage.MessageText.Substring(0, 100);

            if (cpMessage.MessageId == default)
                context.Messages.Add((DbMessage)cpMessage);
            else
            {
                var oldMessage = context.Messages.FirstOrDefault(m => m.MessageId == cpMessage.MessageId);

                if (oldMessage is null)
                    context.Messages.Add((DbMessage)cpMessage);
                else
                    oldMessage.ParseFrom(cpMessage);
            }

            var isSaved = context.SaveChanges() == 1;
            message.MessageId = cpMessage.MessageId;

            return isSaved;
        }

        public static bool SaveToDb(this IUser user, ZsBotDbContext context)
        {
            if (user == null)
                throw new ArgumentNullException(nameof(user));

            if (context == null)
                throw new ArgumentNullException(nameof(context));

            if (!context.Users.Any(u => u.RawDataHash == user.RawDataHash))
                context.Users.Add((DbUser)user);

            return context.SaveChanges() == 1;
        }

        public static bool UpdateRawData(this IChat chat, ZsBotDbContext context)
        {
            if (chat is null)
                throw new ArgumentNullException(nameof(chat));

            if (context == null)
                throw new ArgumentNullException(nameof(context));

            var originalChat = context.Chats.FirstOrDefault(u => u.ChatId == chat.ChatId);

            if (originalChat is null)
                throw new ArgumentOutOfRangeException(nameof(chat.ChatId));

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

                originalChat.ChatName = chat.ChatName;
                originalChat.ChatDescription = chat.ChatDescription;
                originalChat.ChatTypeCode = chat.ChatTypeCode;
                originalChat.RawData = chat.RawData;
                originalChat.RawDataHash = originalChat.RawData.GetMD5Hash();

                return context.SaveChanges() == 1;
            }
        }

        public static bool UpdateRawData(this IMessage message, ZsBotDbContext context)
        {
            if (message is null)
                throw new ArgumentNullException(nameof(message));

            if (context == null)
                throw new ArgumentNullException(nameof(context));

            var originalMessage = context.Messages.FirstOrDefault(m => m.MessageId == message.MessageId);

            if (originalMessage is null)
                throw new ArgumentOutOfRangeException(nameof(message.MessageId));

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

                originalMessage.MessageText = message.MessageText?.Length > 100
                                            ? message.MessageText.Substring(0, 100)
                                            : message.MessageText;
                originalMessage.MessageTypeCode = message.MessageTypeCode;
                originalMessage.ReplyToMessageId = message.ReplyToMessageId;
                originalMessage.RawData = message.RawData;
                originalMessage.RawDataHash = originalMessage.RawData.GetMD5Hash();

                return context.SaveChanges() == 1;
            }
        }

        public static bool UpdateRawData(this IUser user, ZsBotDbContext context)
        {
            if (user is null)
                throw new ArgumentNullException(nameof(user));

            if (context is null)
                throw new ArgumentNullException(nameof(context));

            var originalUser = context.Users.FirstOrDefault(u => u.UserId == user.UserId);

            if (originalUser is null)
                throw new ArgumentOutOfRangeException(nameof(user.UserId));

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

                originalUser.UserName = user.UserName;
                originalUser.UserFullName = user.UserFullName;
                originalUser.RawData = user.RawData;
                originalUser.RawDataHash = originalUser.RawData.GetMD5Hash();

                return context.SaveChanges() == 1;
            }
        }

        public static int GetActualId(this IChat chat, ZsBotDbContext context)
        {
            if (chat == null)
                throw new ArgumentNullException(nameof(chat));

            if (context == null)
                throw new ArgumentNullException(nameof(context));

            var dbChat = context.Chats.FirstOrDefault(c => c.RawDataHash == chat.RawDataHash);

            return dbChat?.ChatId ?? throw new ItemNotFoundException(chat);
        }

        public static int GetActualId(this IUser user, ZsBotDbContext context)
        {
            if (user == null)
                throw new ArgumentNullException(nameof(user));

            var dbUser = context.Users
                .FirstOrDefault(u => u.RawDataHash == user.RawDataHash);

            return dbUser?.UserId ?? throw new ItemNotFoundException(user);
        }

        public static DbChat GetChat(int chatId, ZsBotDbContext context)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));

            var dbChat = context.Chats.FirstOrDefault(c => c.ChatId == chatId);

            return dbChat ?? throw new ItemNotFoundException();
        }

        public static void ParseFrom(this IMessage message, IMessage sourceMessage)
        {
            message.MessageId        = sourceMessage.MessageId;
            message.ReplyToMessageId = sourceMessage.ReplyToMessageId;
            message.MessengerCode    = sourceMessage.MessengerCode;
            message.MessageTypeCode  = sourceMessage.MessageTypeCode;
            message.UserId           = sourceMessage.UserId;
            message.ChatId           = sourceMessage.ChatId;
            message.MessageText      = sourceMessage.MessageText;
            message.RawData          = sourceMessage.RawData;
            message.IsSucceed        = sourceMessage.IsSucceed;
            message.FailsCount       = sourceMessage.FailsCount;
            message.FailDescription  = sourceMessage.FailDescription;
            message.IsDeleted        = sourceMessage.IsDeleted;
            message.InsertDate       = sourceMessage.InsertDate;
            message.UpdateDate       = DateTime.Now;
        }
    }

    public partial class DbUserRoleExtensions
    {
        public static string[] GetPermissionsArray(string userRoleCode, ZsBotDbContext context)
        {
            if (userRoleCode == null)
                throw new ArgumentNullException(nameof(userRoleCode));

            if (context == null)
                throw new ArgumentNullException(nameof(context));

            var permissions = context.UserRoles.FirstOrDefault(r => r.UserRoleCode == userRoleCode)?.UserRolePermissions;

            return JsonSerializer.Deserialize<string[]>(permissions);
        }
    }

    public static class DbLogExtensions
    {
        private static int _logMessageMaxLength = -1;

        public static bool SaveToDb(LogType type, string message, ZsBotDbContext context, string initiator = null, string data = null)
        {
            try
            {
                if (_logMessageMaxLength == -1)
                {
                    var attribute = (StringLengthAttribute)typeof(DbLog).GetProperty(nameof(DbLog.LogMessage)).GetCustomAttributes(true)
                        .FirstOrDefault(a => a is StringLengthAttribute);
                    _logMessageMaxLength = attribute?.MaximumLength ?? 100;
                }

                if (message.Length > _logMessageMaxLength)
                    message = message.Substring(0, _logMessageMaxLength - 3) + "...";

                context.Logs.Add(new DbLog
                {
                    LogType = type.ToString(),
                    LogMessage = message,
                    LogInitiator = initiator,
                    LogData = data,
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
