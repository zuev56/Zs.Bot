using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Zs.Bot.Data.Abstractions;

namespace Zs.Bot.Data.Models
{
    [Table("message_types", Schema = "bot")]
    public class MessageType : IDbEntity<MessageType, string>
    {
        [Key]
        [StringLength(3)]
        [Required(ErrorMessage = "Property 'Id' is required")]
        [Column("message_type_code", TypeName = "character varying(3)")]
        public string Id { get; set; }

        [StringLength(50)]
        [Required(ErrorMessage = "Property 'MessageTypeName' is required")]
        [Column("message_type_name", TypeName = "character varying(50)")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Property 'UpdateDate' is required")]
        [Column("update_date", TypeName = "timestamp with time zone")]
        public DateTime UpdateDate { get; set; }

        [Required(ErrorMessage = "Property 'InsertDate' is required")]
        [Column("insert_date", TypeName = "timestamp with time zone")]
        public DateTime InsertDate { get; set; }

        public Func<MessageType> GetItemToSave => () => this;
        public Func<MessageType, MessageType> GetItemToUpdate => (existingItem) => this;
        
        public ICollection<Message> Messages { get; set; }
    }

}
