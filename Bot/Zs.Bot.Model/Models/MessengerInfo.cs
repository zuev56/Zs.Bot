using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Zs.Bot.Data.Abstractions;

namespace Zs.Bot.Data.Models
{
    [Table("messengers", Schema = "bot")]
    public class MessengerInfo : IDbEntity<MessengerInfo, string>
    {
        [Key]
        [StringLength(2)]
        [Required(ErrorMessage = "Property 'Id' is required")]
        [Column("messenger_code", TypeName = "character varying(2)")]
        public string Id { get; set; }

        [StringLength(20)]
        [Required(ErrorMessage = "Property 'MessengerName' is required")]
        [Column("messenger_name", TypeName = "character varying(20)")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Property 'UpdateDate' is required")]
        [Column("update_date", TypeName = "timestamp with time zone")]
        public DateTime UpdateDate { get; set; }

        [Required(ErrorMessage = "Property 'InsertDate' is required")]
        [Column("insert_date", TypeName = "timestamp with time zone")]
        public DateTime InsertDate { get; set; }

        public Func<MessengerInfo> GetItemToSave => () => this;
        public Func<MessengerInfo, MessengerInfo> GetItemToUpdate => (existingItem) => this;

        public ICollection<Message> Messages { get; set; }

    }

}
