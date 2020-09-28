using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Zs.Bot.Model;
using Zs.App.ChatAdmin.Abstractions;

namespace Zs.App.ChatAdmin.Model
{
    /// <summary> Информация о банах </summary>
    [Table("bans", Schema = "zl")]
    public partial class Ban : IBan
    {
        [Key]
        [Required(ErrorMessage = "Property 'BanId' is required")]
        [Column("ban_id", TypeName = "integer")]
        public int Id { get; set; }

        [Required(ErrorMessage = "Property 'UserId' is required")]
        [Column("user_id", TypeName = "integer")]
        public int UserId { get; set; }

        [Required(ErrorMessage = "Property 'ChatId' is required")]
        [Column("chat_id", TypeName = "integer")]
        public int ChatId { get; set; }

        [Column("warning_message_id", TypeName = "integer")]
        public int? WarningMessageId { get; set; }

        [Column("ban_finish_date", TypeName = "timestamp with time zone")]
        public DateTime? FinishDate { get; set; }

        [Required(ErrorMessage = "Property 'UpdateDate' is required")]
        [Column("update_date", TypeName = "timestamp with time zone")]
        public DateTime UpdateDate { get; set; }

        [Required(ErrorMessage = "Property 'InsertDate' is required")]
        [Column("insert_date", TypeName = "timestamp with time zone")]
        public DateTime InsertDate { get; set; }

        public User User { get; set; }
        public Chat Chat { get; set; }
        public Message Message { get; set; }
    }

}