using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Zs.Bot.Data.Abstractions;

namespace Zs.Bot.Data.Models
{
    [Table("chat_types", Schema = "bot")]
    public class ChatType : IDbEntity<ChatType, string>
    {
        [Key]
        [StringLength(10)]
        [Required(ErrorMessage = "Property 'Id' is required")]
        [Column("chat_type_code", TypeName = "character varying(10)")]
        public string Id { get; set; }

        [StringLength(10)]
        [Required(ErrorMessage = "Property 'ChatTypeName' is required")]
        [Column("chat_type_name", TypeName = "character varying(10)")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Property 'UpdateDate' is required")]
        [Column("update_date", TypeName = "timestamp with time zone")]
        public DateTime UpdateDate { get; set; }

        [Required(ErrorMessage = "Property 'InsertDate' is required")]
        [Column("insert_date", TypeName = "timestamp with time zone")]
        public DateTime InsertDate { get; set; }
        public Func<ChatType> GetItemToSave => () => this;
        public Func<ChatType, ChatType> GetItemToUpdate => (existingItem) => this;

        public ICollection<Chat> Chats { get; set; }
    }

}
