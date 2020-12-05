using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Zs.Bot.Data.Abstractions;

namespace Zs.Bot.Data.Models
{
    [Table("chats", Schema = "bot")]
    public class Chat : IDbEntityWithRawData<Chat, int>
    {
        [Key]
        [Required(ErrorMessage = "Property 'ChatId' is required")]
        [Column("chat_id", TypeName = "int")]
        public int Id { get; set; }

        [StringLength(50)]
        [Required(ErrorMessage = "Property 'ChatName' is required")]
        [Column("chat_name", TypeName = "character varying(50)")]
        public string Name { get; set; }

        [StringLength(100)]
        [Column("chat_description", TypeName = "character varying(100)")]
        public string Description { get; set; }

        [StringLength(10)]
        [Required(ErrorMessage = "Property 'ChatTypeId' is required")]
        [Column("chat_type_code", TypeName = "character varying(10)")]
        public string ChatTypeId { get; set; }

        [Required(ErrorMessage = "Property 'RawData' is required")]
        [Column("raw_data", TypeName = "json")]
        public string RawData { get; set; }

        [StringLength(50)]
        [Required(ErrorMessage = "Property 'RawDataHash' is required")]
        [Column("raw_data_hash", TypeName = "character varying(50)")]
        public string RawDataHash { get; set; }

        [Column("raw_data_history", TypeName = "json")]
        public string RawDataHistory { get; set; }

        [Required(ErrorMessage = "Property 'UpdateDate' is required")]
        [Column("update_date", TypeName = "timestamp with time zone")]
        public DateTime UpdateDate { get; set; }

        [Required(ErrorMessage = "Property 'InsertDate' is required")]
        [Column("insert_date", TypeName = "timestamp with time zone")]
        public DateTime InsertDate { get; set; }

        public Func<Chat> GetItemToSave => () => this;
        public Func<Chat, Chat> GetItemToUpdate => (existingItem) =>
            new Chat
            {
                Id             = existingItem.Id,
                Name           = this.Name,
                Description    = this.Description,
                ChatTypeId     = this.ChatTypeId,
                RawData        = this.RawData,
                RawDataHash    = this.RawDataHash,
                RawDataHistory = this.RawDataHistory,
                UpdateDate     = DateTime.Now,
                InsertDate     = existingItem.InsertDate
            };


        public ChatType ChatType { get; set; }
        public ICollection<Message> Messages { get; set; }
        
        public override bool Equals(object obj)
        {
            return Equals(obj as Chat);
        }

        public bool Equals(Chat other)
        {
            return other != null &&
                   Id == other.Id &&
                   Name == other.Name &&
                   Description == other.Description &&
                   ChatTypeId == other.ChatTypeId &&
                   RawData == other.RawData &&
                   RawDataHash == other.RawDataHash &&
                   RawDataHistory == other.RawDataHistory;
        }

        public override int GetHashCode()
        {
            HashCode hash = new HashCode();
            hash.Add(Id);
            hash.Add(Name);
            hash.Add(Description);
            hash.Add(ChatTypeId);
            hash.Add(RawData);
            hash.Add(RawDataHash);
            hash.Add(RawDataHistory);
            return hash.ToHashCode();
        }

    }

}
