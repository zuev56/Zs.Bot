using System;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Unicode;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Zs.Bot.Model.Db;
using Zs.Bot.Modules.Messaging;
using Zs.Common.Extensions;
using TgChatType = Telegram.Bot.Types.Enums.ChatType;

namespace Zs.Bot.Telegram
{
    internal class ItemConverter : IToGenegalItemConverter
    {
        /// <inheritdoc />
        public IMessage ToGeneralMessage(object specificMessage)
        {
            var options = new JsonSerializerOptions
            {
                IgnoreNullValues = true,
                WriteIndented = true,
                Encoder = JavaScriptEncoder.Create(UnicodeRanges.All)
            };

            if (specificMessage is TgMessage telegramMessage)
            {
                var message = DbEntityFactory.NewMessage();
                
                //message.MessageId     -> Auto
                //message.ChatId        -> define when saving
                //message.UserId        -> define when saving
                message.MessengerCode = "TG";
                message.MessageTypeCode = GetGeneralMessageTypeCode(telegramMessage.Type);
                message.MessageText = telegramMessage.Text;
                message.RawData = JsonSerializer.Serialize(telegramMessage, options).NormalizeJsonString();
                message.RawDataHash = message.RawData.GetMD5Hash();
                message.IsSucceed = telegramMessage.IsSucceed;
                message.FailsCount = telegramMessage.SendingFails;
                message.FailDescription = telegramMessage.FailDescription;
                message.ReplyToMessageId = null; // Надо сначала найти в БД подходящее сообщение и указать его MessageId
                return message;
            }
            else
                throw new InvalidCastException($"{nameof(specificMessage)} is not a {typeof(Message).FullName}");
        }

        /// <inheritdoc />
        public IChat ToGeneralChat(object specificChat)
        {
            var options = new JsonSerializerOptions
            {
                IgnoreNullValues = true,
                WriteIndented = true,
                Encoder = JavaScriptEncoder.Create(UnicodeRanges.All)
            };

            if (specificChat is Chat telegramChat)
            {
                var chat = DbEntityFactory.NewChat();

                
                //chat.ChatId -> Auto
                chat.ChatDescription = telegramChat.Description;
                chat.ChatName = telegramChat.Title ?? telegramChat.Username ?? $"{telegramChat.FirstName} {telegramChat.LastName}";
                chat.ChatTypeCode = ToGeneralChatType(telegramChat.Type).ToString().ToUpperInvariant();
                chat.RawData = JsonSerializer.Serialize(telegramChat, options).NormalizeJsonString();
                chat.RawDataHash = chat.RawData.GetMD5Hash();

                return chat;
            }
            else
                throw new InvalidCastException($"{nameof(specificChat)} is not a {typeof(Chat).FullName}");
        }

        /// <inheritdoc />
        public IUser ToGeneralUser(object specificUser)
        {
            var options = new JsonSerializerOptions
            {
                IgnoreNullValues = true,
                WriteIndented = true,
                Encoder = JavaScriptEncoder.Create(UnicodeRanges.All)
            };

            if (specificUser is User telegramUser)
            {
                var user = DbEntityFactory.NewUser();

                //user.UserId -> Auto
                user.UserRoleCode = "USER";
                user.UserName = telegramUser.Username;
                user.UserFullName = $"{telegramUser.FirstName} {telegramUser.LastName}".Trim();
                user.UserIsBot = telegramUser.IsBot;
                user.RawData = JsonSerializer.Serialize(telegramUser, options).NormalizeJsonString();
                user.RawDataHash = user.RawData.GetMD5Hash();

                return user;
            }
            else
                throw new InvalidCastException($"{nameof(specificUser)} is not a {typeof(User).FullName}");
        }

        /// <inheritdoc />
        public Common.Enums.ChatType ToGeneralChatType(object specificChatType)
        {
            if (specificChatType is TgChatType chatType)
            {
                return chatType switch
                {
                    TgChatType.Group      => Common.Enums.ChatType.Group,
                    TgChatType.Supergroup => Common.Enums.ChatType.Group,
                    TgChatType.Channel    => Common.Enums.ChatType.Channel,
                    TgChatType.Private    => Common.Enums.ChatType.Private,
                    _ => Common.Enums.ChatType.Undefined
                };
            }
            else
                return Common.Enums.ChatType.Undefined;
        }

        private static string GetGeneralMessageTypeCode(MessageType type)
        {
            return type switch
            {
                MessageType.Text     => "TXT",
                MessageType.Photo    => "PHT",
                MessageType.Audio    => "AUD",
                MessageType.Video    => "VID",
                MessageType.Voice    => "VOI",
                MessageType.Document => "DOC",
                MessageType.Sticker  => "STK",
                MessageType.Location => "LOC",
                MessageType.Contact  => "CNT",

                var o when
                o == MessageType.Venue ||
                o == MessageType.Game ||
                o == MessageType.VideoNote ||
                o == MessageType.Invoice ||
                o == MessageType.SuccessfulPayment ||
                o == MessageType.WebsiteConnected ||
                o == MessageType.Animation ||
                o == MessageType.Poll ||
                o == MessageType.Dice => "OTH",

                var s when
                s == MessageType.ChatMembersAdded ||
                s == MessageType.ChatMemberLeft ||
                s == MessageType.ChatTitleChanged ||
                s == MessageType.ChatPhotoChanged ||
                s == MessageType.MessagePinned ||
                s == MessageType.ChatPhotoDeleted ||
                s == MessageType.GroupCreated ||
                s == MessageType.SupergroupCreated ||
                s == MessageType.ChannelCreated ||
                s == MessageType.MigratedToSupergroup ||
                s == MessageType.MigratedFromGroup => "SRV",

                _ => "UKN"
            };
        }
    }
}
