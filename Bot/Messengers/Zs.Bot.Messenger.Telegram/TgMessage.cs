using System;
using System.Text.Json.Serialization;
using Telegram.Bot.Types;
using Zs.Bot.Data.Abstractions;

namespace Zs.Bot.Messenger.Telegram
{
    /// <summary>
    /// Обёртка входящего сообщения Telegram.
    /// Сделана по большей части для удобства и на перспективу
    /// </summary>
    internal class TgMessage : Message
    {
        [JsonIgnore]
        public new User From { get => base.From; set => base.From = value; }
        
        [JsonIgnore]
        public new User ForwardFrom { get => base.ForwardFrom; set => base.ForwardFrom = value; }

        [JsonIgnore]
        public new bool IsForwarded { get => base.IsForwarded; }

        [JsonIgnore]
        public new int ForwardFromMessageId { get => base.ForwardFromMessageId; set => base.ForwardFromMessageId = value; }

        [JsonIgnore]
        public new Chat Chat { get => base.Chat; set => base.Chat = value; }
        
        [JsonIgnore]
        public new Chat ForwardFromChat { get => base.ForwardFromChat; set => base.ForwardFromChat = value; }
        
        [JsonIgnore]
        public new Message ReplyToMessage { get => base.ReplyToMessage; set => base.ReplyToMessage = value; }

        [JsonIgnore]
        public new Message PinnedMessage { get => base.PinnedMessage; set => base.PinnedMessage = value; }
        

        [JsonIgnore]
        public new bool DeleteChatPhoto { get => base.DeleteChatPhoto; set => base.DeleteChatPhoto = value; }

        [JsonIgnore]
        public new bool GroupChatCreated { get => base.GroupChatCreated; set => base.GroupChatCreated = value; }

        [JsonIgnore]
        public new bool SupergroupChatCreated { get => base.SupergroupChatCreated; set => base.SupergroupChatCreated = value; }

        [JsonIgnore]
        public new bool ChannelChatCreated { get => base.ChannelChatCreated; set => base.ChannelChatCreated = value; }

        [JsonIgnore]
        public new long MigrateToChatId { get => base.MigrateToChatId; set => base.MigrateToChatId = value; }
        
        [JsonIgnore]
        public new long MigrateFromChatId { get => base.MigrateFromChatId; set => base.MigrateFromChatId = value; }

        [JsonIgnore]
        public bool IsEdited { get; set; }

        [JsonIgnore]
        public int SendingFails { get; set; } // Счётчик неудачных попыток посылки сообщения

        [JsonIgnore] 
        public string FailDescription { get; set; } // Описание проблемы, из-за которой не удалось отправить сообщение

        [JsonIgnore] 
        public bool IsSucceed { get; set; }

        public int FromId => base.From.Id;
        public int? ForwardFromId => base.ForwardFrom?.Id; 
        public long ChatId => base.Chat.Id;
        public long? ForwardFromChatId => base.ForwardFromChat?.Id;
        public int? ReplyToMessageId => base.ReplyToMessage?.MessageId;
        public int? PinnedMessageId => base.PinnedMessage?.MessageId;



        public TgMessage(Message msg)
        {
            Parse(msg ?? throw new ArgumentNullException(nameof(msg)));

            Date = Date == default ? DateTime.Now : Date;
        }

        public TgMessage(Chat chat, string msgText)
        {
            Parse(new Message()
            {
                Chat = chat ?? throw new ArgumentNullException(nameof(chat)),
                Text = msgText
            });

            Date = DateTime.Now;
        }


        /// <summary> Наполнение полей класса данными оригинального сообщения </summary>
        internal void Parse(Message msg)
        {
            Animation             = msg.Animation;
            Audio                 = msg.Audio;
            AuthorSignature       = msg.AuthorSignature;
            Caption               = msg.Caption;
            CaptionEntities       = msg.CaptionEntities;
            ChannelChatCreated    = msg.ChannelChatCreated;
            base.Chat             = msg.Chat;
            ConnectedWebsite      = msg.ConnectedWebsite;
            Contact               = msg.Contact;
            Date                  = msg.Date;
            DeleteChatPhoto       = msg.DeleteChatPhoto;
            Document              = msg.Document;
            EditDate              = msg.EditDate;
            Entities              = msg.Entities;
            ForwardDate           = msg.ForwardDate;
            base.ForwardFrom      = msg.ForwardFrom;
            base.ForwardFromChat  = msg.ForwardFromChat;
            ForwardFromMessageId  = msg.ForwardFromMessageId;
            ForwardSignature      = msg.ForwardSignature;
            base.From             = msg.From;
            Game                  = msg.Game;
            GroupChatCreated      = msg.GroupChatCreated;
            Invoice               = msg.Invoice;
            LeftChatMember        = msg.LeftChatMember;
            Location              = msg.Location;
            MediaGroupId          = msg.MediaGroupId;
            MessageId             = msg.MessageId;
            MigrateFromChatId     = msg.MigrateFromChatId;
            MigrateToChatId       = msg.MigrateToChatId;
            NewChatMembers        = msg.NewChatMembers;
            NewChatPhoto          = msg.NewChatPhoto;
            NewChatTitle          = msg.NewChatTitle;
            Photo                 = msg.Photo;
            PinnedMessage         = msg.PinnedMessage;
            ReplyToMessage        = msg.ReplyToMessage;
            Sticker               = msg.Sticker;
            SuccessfulPayment     = msg.SuccessfulPayment;
            SupergroupChatCreated = msg.SupergroupChatCreated;
            Text                  = msg.Text;
            Venue                 = msg.Venue;
            Video                 = msg.Video;
            VideoNote             = msg.VideoNote;
            Voice                 = msg.Voice;
        }
    }
}
