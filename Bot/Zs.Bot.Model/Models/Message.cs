using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Zs.Bot.Data.Abstractions;

namespace Zs.Bot.Data.Models
{
    [Table("messages", Schema = "bot")]
    public class Message : IDbEntityWithRawData<Message, int>
    {
        [Key]
        [Required(ErrorMessage = "Property 'MessageId' is required")]
        [Column("message_id", TypeName = "int")]
        public int Id { get; set; }

        [Column("reply_to_message_id", TypeName = "integer")]
        public int? ReplyToMessageId { get; set; }

        [StringLength(2)]
        [Required(ErrorMessage = "Property 'MessengerId' is required")]
        [Column("messenger_code", TypeName = "character varying(2)")]
        public string MessengerId { get; set; }

        [StringLength(3)]
        [Required(ErrorMessage = "Property 'MessageTypeId' is required")]
        [Column("message_type_code", TypeName = "character varying(3)")]
        public string MessageTypeId { get; set; }

        [Required(ErrorMessage = "Property 'UserId' is required")]
        [Column("user_id", TypeName = "integer")]
        public int UserId { get; set; }

        [Required(ErrorMessage = "Property 'ChatId' is required")]
        [Column("chat_id", TypeName = "integer")]
        public int ChatId { get; set; }

        [StringLength(100)]
        [Column("message_text", TypeName = "character varying(100)")]
        public string Text { get; set; }

        [Required(ErrorMessage = "Property 'RawData' is required")]
        [Column("raw_data", TypeName = "json")]
        public string RawData { get; set; }

        [StringLength(50)]
        [Required(ErrorMessage = "Property 'RawDataHash' is required")]
        [Column("raw_data_hash", TypeName = "character varying(50)")]
        public string RawDataHash { get; set; }

        [Column("raw_data_history", TypeName = "json")]
        public string RawDataHistory { get; set; }

        [Required(ErrorMessage = "Property 'IsSucceed' is required")]
        [Column("is_succeed", TypeName = "bool")]
        public bool IsSucceed { get; set; }

        [Required(ErrorMessage = "Property 'FailsCount' is required")]
        [Column("fails_count", TypeName = "integer")]
        public int FailsCount { get; set; }

        [Column("fail_description", TypeName = "json")]
        public string FailDescription { get; set; }

        [Required(ErrorMessage = "Property 'IsDeleted' is required")]
        [Column("is_deleted", TypeName = "bool")]
        public bool IsDeleted { get; set; }

        [Required(ErrorMessage = "Property 'UpdateDate' is required")]
        [Column("update_date", TypeName = "timestamp with time zone")]
        public DateTime UpdateDate { get; set; }

        [Required(ErrorMessage = "Property 'InsertDate' is required")]
        [Column("insert_date", TypeName = "timestamp with time zone")]
        public DateTime InsertDate { get; set; }

        public MessageType MessageType { get; set; }
        public Message ReplyToMessage { get; set; }
        public MessengerInfo Messenger { get; set; }
        public User User { get; set; }
        public Chat Chat { get; set; }

        public Func<Message> GetItemToSave => () =>
            new Message
            {
                Id               = this.Id,
                ReplyToMessageId = this.ReplyToMessageId,
                MessengerId      = this.MessengerId,
                MessageTypeId    = this.MessageTypeId,
                UserId           = this.UserId,
                ChatId           = this.ChatId,
                Text             = this.Text?.Length > 100 ? this.Text.Substring(0, 100) : this.Text,
                RawData          = this.RawData,
                RawDataHash      = this.RawDataHash,
                RawDataHistory   = this.RawDataHistory,
                IsSucceed        = this.IsSucceed,
                FailsCount       = this.FailsCount,
                FailDescription  = this.FailDescription,
                IsDeleted        = this.IsDeleted,
                UpdateDate       = this.UpdateDate,
                InsertDate       = this.InsertDate,
            };

        public Func<Message, Message> GetItemToUpdate => (existingItem) =>
            new Message
            {
                Id               = existingItem.Id,
                ReplyToMessageId = this.ReplyToMessageId,
                MessengerId      = existingItem.MessengerId,
                MessageTypeId    = this.MessageTypeId,
                UserId           = existingItem.UserId,
                ChatId           = existingItem.ChatId,
                Text             = this.Text,
                RawData          = this.RawData,
                RawDataHash      = this.RawDataHash,
                RawDataHistory   = this.RawDataHistory,
                IsSucceed        = existingItem.IsSucceed,
                FailsCount       = existingItem.FailsCount,
                FailDescription  = existingItem.FailDescription,
                IsDeleted        = existingItem.IsDeleted,
                UpdateDate       = DateTime.Now,
                InsertDate       = existingItem.InsertDate,
            };
        
        public override bool Equals(object obj)
        {
            return Equals(obj as Message);
        }

        public bool Equals(Message other)
        {
            return other != null &&
                   Id == other.Id &&
                   ReplyToMessageId == other.ReplyToMessageId &&
                   MessengerId == other.MessengerId &&
                   MessageTypeId == other.MessageTypeId &&
                   UserId == other.UserId &&
                   ChatId == other.ChatId &&
                   Text == other.Text &&
                   RawData == other.RawData &&
                   RawDataHash == other.RawDataHash &&
                   RawDataHistory == other.RawDataHistory &&
                   IsSucceed == other.IsSucceed &&
                   FailsCount == other.FailsCount &&
                   FailDescription == other.FailDescription &&
                   IsDeleted == other.IsDeleted;
        }

        public override int GetHashCode()
        {
            HashCode hash = new HashCode();
            hash.Add(Id);
            hash.Add(ReplyToMessageId);
            hash.Add(MessengerId);
            hash.Add(MessageTypeId);
            hash.Add(UserId);
            hash.Add(ChatId);
            hash.Add(Text);
            hash.Add(RawData);
            hash.Add(RawDataHash);
            hash.Add(RawDataHistory);
            hash.Add(IsSucceed);
            hash.Add(FailsCount);
            hash.Add(FailDescription);
            hash.Add(IsDeleted);
            return hash.ToHashCode();
        }

        public Message DeepCopy()
        {
            return new Message
            {
                Id               = this.Id,
                ReplyToMessageId = this.ReplyToMessageId,
                MessengerId      = this.MessengerId,
                MessageTypeId    = this.MessageTypeId,
                UserId           = this.UserId,
                ChatId           = this.ChatId,
                Text             = this.Text,
                RawData          = this.RawData,
                RawDataHash      = this.RawDataHash,
                RawDataHistory   = this.RawDataHistory,
                IsSucceed        = this.IsSucceed,
                FailsCount       = this.FailsCount,
                FailDescription  = this.FailDescription,
                IsDeleted        = this.IsDeleted,
                UpdateDate       = this.UpdateDate,
                InsertDate       = this.InsertDate,
            };
        }
    }

}
