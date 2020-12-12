using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Zs.Bot.Data.Abstractions;

namespace Zs.Bot.Data.Models
{
    [Table("commands", Schema = "bot")]
    public class Command : IDbEntity<Command, string>
    {
        [Key]
        [StringLength(50)]
        [Required(ErrorMessage = "Property 'CommandName' is required")]
        [Column("command_name", TypeName = "character varying(50)")]
        public string Id { get; set; }

        [StringLength(5000)]
        [Required(ErrorMessage = "Property 'CommandScript' is required")]
        [Column("command_script", TypeName = "character varying(5000)")]
        public string Script { get; set; }

        [StringLength(100)]
        [Column("command_default_args", TypeName = "character varying(100)")]
        public string DefaultArgs { get; set; }

        [StringLength(100)]
        [Column("command_desc", TypeName = "character varying(100)")]
        public string Description { get; set; }

        [StringLength(50)]
        [Required(ErrorMessage = "Property 'CommandGroup' is required")]
        [Column("command_group", TypeName = "character varying(50)")]
        public string Group { get; set; }

        [Required(ErrorMessage = "Property 'UpdateDate' is required")]
        [Column("update_date", TypeName = "timestamp with time zone")]
        public DateTime UpdateDate { get; set; }

        [Required(ErrorMessage = "Property 'InsertDate' is required")]
        [Column("insert_date", TypeName = "timestamp with time zone")]
        public DateTime InsertDate { get; set; }

        public Func<Command> GetItemToSave => () => this;
        public Func<Command, Command> GetItemToUpdate => (existingItem) => this;

    }

}
