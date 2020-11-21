using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using Zs.App.ChatAdmin.Abstractions;

namespace Zs.App.ChatAdmin.Model
{
    /// <summary> Напоминание о событиях </summary>
    [Table("notifications", Schema = "zl")]
    public partial class Notification : INotification
    {
        [Key]
        [Required(ErrorMessage = "Property 'NotificationId' is required")]
        [Column("notification_id", TypeName = "integer")]
        public int Id { get; set; }

        [Required(ErrorMessage = "Property 'NotificationIsActive' is required")]
        [Column("notification_is_active", TypeName = "boolean")]
        public bool IsActive { get; set; }

        [StringLength(2000)]
        [Required(ErrorMessage = "Property 'NotificationMessage' is required")]
        [Column("notification_message", TypeName = "character varying(2000)")]
        public string Message { get; set; }

        [Column("notification_month", TypeName = "integer")]
        public int? Month { get; set; }

        [Required(ErrorMessage = "Property 'NotificationDay' is required")]
        [Column("notification_day", TypeName = "integer")]
        public int Day { get; set; }

        [Required(ErrorMessage = "Property 'NotificationHour' is required")]
        [Column("notification_hour", TypeName = "integer")]
        public int Hour { get; set; }

        [Required(ErrorMessage = "Property 'NotificationMinute' is required")]
        [Column("notification_minute", TypeName = "integer")]
        public int Minute { get; set; }

        [Column("notification_exec_date", TypeName = "timestamp with time zone")]
        public DateTime? ExecDate { get; set; }

        [Required(ErrorMessage = "Property 'UpdateDate' is required")]
        [Column("update_date", TypeName = "timestamp with time zone")]
        public DateTime UpdateDate { get; set; }

        [Required(ErrorMessage = "Property 'InsertDate' is required")]
        [Column("insert_date", TypeName = "timestamp with time zone")]
        public DateTime InsertDate { get; set; }
    }
}