using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Zs.App.Home.Model.Abstractions;

namespace Zs.App.Home.Model
{
    /// <summary> <inheritdoc/> </summary>
    [Table("activity_log", Schema = "vk")]
    public partial class VkActivityLog : IVkActivityLog
    {
        [Key]
        [Required(ErrorMessage = "Property 'ActivityLogId' is required")]
        [Column("activity_log_id", TypeName = "integer")]
        public int Id { get; set; }

        [Required(ErrorMessage = "Property 'UserId' is required")]
        [Column("user_id", TypeName = "integer")]
        public int UserId { get; set; }

        [Column("is_online", TypeName = "boolean")]
        public bool? IsOnline { get; set; }

        [Required(ErrorMessage = "Property 'InsertDate' is required")]
        [Column("insert_date", TypeName = "timestamp with time zone")]
        public DateTime InsertDate { get; set; }

        [Column("online_app", TypeName = "integer")]
        public int? OnlineApp { get; set; }

        [Required(ErrorMessage = "Property 'IsOnlineMobile' is required")]
        [Column("is_online_mobile", TypeName = "boolean")]
        public bool IsOnlineMobile { get; set; }

        [Required(ErrorMessage = "Property 'LastSeen' is required")]
        [Column("last_seen", TypeName = "integer")]
        public int LastSeen { get; set; }

        public VkUser VkUser { get; set; }
    }
}
