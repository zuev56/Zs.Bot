using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using Zs.Bot.Data.Abstractions;

namespace Zs.App.Home.Data.Models.Vk
{
    /// <summary> Vk users activity log item </summary>
    [Table("activity_log", Schema = "vk")]
    public partial class ActivityLogItem : IDbEntity<ActivityLogItem, int>
    {
        [Key]
        [Column("activity_log_id", TypeName = "integer")]
        public int Id { get; set; }

        [Required(ErrorMessage = "Property 'UserId' is required")]
        [Column("user_id", TypeName = "integer")]
        public int UserId { get; set; }

        [Column("is_online", TypeName = "boolean")]
        public bool? IsOnline { get; set; }

        [Column("insert_date", TypeName = "timestamp with time zone")]
        public DateTime InsertDate { get; set; }

        [Column("online_app", TypeName = "integer")]
        public int? OnlineApp { get; set; }

        [Column("is_online_mobile", TypeName = "boolean")]
        public bool IsOnlineMobile { get; set; }

        [Column("last_seen", TypeName = "integer")]
        public int LastSeen { get; set; }
        [JsonIgnore]
        public Func<ActivityLogItem> GetItemToSave => () => this;
        [JsonIgnore]
        public Func<ActivityLogItem, ActivityLogItem> GetItemToUpdate => (existingItem) => this;


        public User User { get; set; }
    }
}
