using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Zs.Bot.Data.Abstractions;

namespace Zs.Bot.Data.Models
{
    [Table("logs", Schema = "bot")]
    public class Log : IDbEntity<Log, int>
    {
        [Key]
        [Required(ErrorMessage = "Property 'LogId' is required")]
        [Column("log_id", TypeName = "int")]
        public int Id { get; set; }

        [StringLength(7)]
        [Required(ErrorMessage = "Property 'LogType' is required")]
        [Column("log_type", TypeName = "character varying(7)")]
        public string Type { get; set; }

        [StringLength(50)]
        [Column("log_initiator", TypeName = "character varying(50)")]
        public string Initiator { get; set; }

        [StringLength(200)]
        [Required(ErrorMessage = "Property 'LogMessage' is required")]
        [Column("log_message", TypeName = "character varying(200)")]
        public string Message { get; set; }

        [Column("log_data", TypeName = "json")]
        public string Data { get; set; }

        [Required(ErrorMessage = "Property 'InsertDate' is required")]
        [Column("insert_date", TypeName = "timestamp with time zone")]
        public DateTime InsertDate { get; set; }

        public Func<Log> GetItemToSave => () => this;
        public Func<Log, Log> GetItemToUpdate => (existingItem) => this;

    }

}
