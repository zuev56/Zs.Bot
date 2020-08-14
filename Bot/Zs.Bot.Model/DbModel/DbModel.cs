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
    public partial class DbLog
    {
        private static int _logMessageMaxLength = -1;

        public static bool SaveToDb(LogType type, string message, string initiator = null, string data = null)
        {
            try
            {
                if (_logMessageMaxLength == -1)
                {
                    var attribute = (StringLengthAttribute)typeof(DbLog).GetProperty(nameof(LogMessage)).GetCustomAttributes(true)
                        .FirstOrDefault(a => a is StringLengthAttribute);
                    _logMessageMaxLength = attribute?.MaximumLength ?? 100;
                }

                if (message.Length > _logMessageMaxLength)
                    message = message.Substring(0, _logMessageMaxLength-3) + "...";
                
                using var ctx = new ZsBotDbContext();
                ctx.Logs.Add(new DbLog
                {
                    LogType      = type.ToString(),
                    LogMessage   = message,
                    LogInitiator = initiator,
                    LogData      = data,
                    InsertDate   = DateTime.Now
                });
                return ctx.SaveChanges() == 1;
            }
            catch
            {
                return false;
            }
        }
    }

    public partial class DbUser
    {
        public static bool SaveToDb(IUser user)
        {
            if (user == null)
                throw new ArgumentNullException(nameof(user));

            using var ctx = new ZsBotDbContext();
            if (!ctx.Users.Any(u => u.RawDataHash == user.RawDataHash))
                ctx.Users.Add((DbUser)user);

            return ctx.SaveChanges() == 1;
        }

        public static bool UpdateRawData(int userId, IUser newUser)
        {
            if (newUser is null)
                throw new ArgumentNullException(nameof(newUser));

            using var ctx = new ZsBotDbContext();
            
            var originalUser = ctx.Users.FirstOrDefault(u => u.UserId == userId);

            if (originalUser is null)
                throw new ArgumentOutOfRangeException(nameof(userId));

            if (originalUser.RawDataHash == newUser.RawDataHash)
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

                originalUser.UserName = newUser.UserName;
                originalUser.UserFullName = newUser.UserFullName;
                originalUser.RawData = newUser.RawData;
                originalUser.RawDataHash = originalUser.RawData.GetMD5Hash();

                return ctx.SaveChanges() == 1;
            }
        }

        public static int GetId(IUser user)
        {
            if (user == null)
                throw new ArgumentNullException(nameof(user));

            using var ctx = new ZsBotDbContext();
            var dbUser = ctx.Users.FirstOrDefault(u => u.RawDataHash == user.RawDataHash);

            return dbUser?.UserId ?? throw new ItemNotFoundException(user);
        }
    }

    public partial class DbChat
    {
        public static bool SaveToDb(IChat chat)
        {
            if (chat == null)
                throw new ArgumentNullException(nameof(chat));

            using var ctx = new ZsBotDbContext();
            if (!ctx.Chats.Any(c => c.RawDataHash == chat.RawDataHash))
                ctx.Chats.Add((DbChat)chat);

            return ctx.SaveChanges() == 1;
        }

        public static int GetId(IChat chat)
        {
            if (chat == null)
                throw new ArgumentNullException(nameof(chat));

            using var ctx = new ZsBotDbContext();
            var dbChat = ctx.Chats.FirstOrDefault(c => c.RawDataHash == chat.RawDataHash);

            return dbChat?.ChatId ?? throw new ItemNotFoundException(chat);
        }

        public static bool UpdateRawData(int chatId, IChat newChat)
        {
            if (newChat is null)
                throw new ArgumentNullException(nameof(newChat));

            using var ctx = new ZsBotDbContext();

            var originalChat = ctx.Chats.FirstOrDefault(u => u.ChatId == chatId);

            if (originalChat is null)
                throw new ArgumentOutOfRangeException(nameof(chatId));

            if (originalChat.RawDataHash == newChat.RawDataHash)
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

                originalChat.ChatName = newChat.ChatName;
                originalChat.ChatDescription = newChat.ChatDescription;
                originalChat.ChatTypeCode = newChat.ChatTypeCode;
                originalChat.RawData = newChat.RawData;
                originalChat.RawDataHash = originalChat.RawData.GetMD5Hash();

                return ctx.SaveChanges() == 1;
            }
        }

        public static DbChat GetChat(int chatId)
        {
            using var ctx = new ZsBotDbContext();
            var dbChat = ctx.Chats.FirstOrDefault(c => c.ChatId == chatId);

            return dbChat ?? throw new ItemNotFoundException();
        }
    }

    public partial class DbMessage
    {
        public static bool SaveToDb(IMessage message)
        {
            if (message == null)
                throw new ArgumentNullException(nameof(message));

            var cpMessage = message.DeepCopy();
            if (cpMessage.MessageText?.Length > 100)
                cpMessage.MessageText = cpMessage.MessageText.Substring(0, 100);

            using var ctx = new ZsBotDbContext();

            if (cpMessage.MessageId == default)
                ctx.Messages.Add((DbMessage)cpMessage);
            else
            {
                var oldMessage = ctx.Messages.FirstOrDefault(m => m.MessageId == cpMessage.MessageId);

                if (oldMessage is null)
                    ctx.Messages.Add((DbMessage)cpMessage);
                else
                    oldMessage.ParseFrom(cpMessage);
            }

            var isSaved = ctx.SaveChanges() == 1;
            message.MessageId = cpMessage.MessageId;
            
            return isSaved;
        }

        public static bool UpdateRawData(int messageId, IMessage newMessage)
        {
            if (newMessage is null)
                throw new ArgumentNullException(nameof(newMessage));

            using var ctx = new ZsBotDbContext();

            var originalMessage = ctx.Messages.FirstOrDefault(m => m.MessageId == messageId);

            if (originalMessage is null)
                throw new ArgumentOutOfRangeException(nameof(messageId));

            if (originalMessage.RawDataHash == newMessage.RawDataHash)
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

                originalMessage.MessageText = newMessage.MessageText;
                originalMessage.MessageTypeCode = newMessage.MessageTypeCode;
                originalMessage.ReplyToMessageId = newMessage.ReplyToMessageId;
                originalMessage.RawData = newMessage.RawData;
                originalMessage.RawDataHash = originalMessage.RawData.GetMD5Hash();

                return ctx.SaveChanges() == 1;
            }
        }

        private void ParseFrom(IMessage message)
        {
            MessageId        = message.MessageId;
            ReplyToMessageId = message.ReplyToMessageId;
            MessengerCode    = message.MessengerCode;
            MessageTypeCode  = message.MessageTypeCode;
            UserId           = message.UserId;
            ChatId           = message.ChatId;
            MessageText      = message.MessageText;
            RawData          = message.RawData;
            IsSucceed        = message.IsSucceed;
            FailsCount       = message.FailsCount;
            FailDescription  = message.FailDescription;
            IsDeleted        = message.IsDeleted;
            InsertDate       = message.InsertDate;
            UpdateDate       = DateTime.Now;
        }
    }

    public partial class DbUserRole
    {
        public static string[] GetPermissionsArray(string userRoleCode)
        {
            if (userRoleCode == null)
                throw new ArgumentNullException(nameof(userRoleCode));

            using var ctx = new ZsBotDbContext();
            var permissions = ctx.UserRoles.FirstOrDefault(r => r.UserRoleCode == userRoleCode)?.UserRolePermissions;

            try
            {
                return JsonSerializer.Deserialize<string[]>(permissions);
            }
            catch (JsonException jex)
            {
                return null;
            }
        }
    }

    /// <summary> Cодержит результат SQL-запроса </summary>
    public partial class DbQuery
    {
        [Key]
        //[Column("QueryResult")]
        public string Result { get; set; }
    }



}
