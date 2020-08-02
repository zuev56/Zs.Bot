﻿using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Zs.Service.ChatAdmin.DbModel
{
    #region Interfaces zl

    /// <summary> Информация о времени начала учёта сообщений каждого отдельного пользователя </summary>
    public interface IAccounting
    {
        public Int32 AccountingId { get; set; }

        public DateTime AccountingStartDate { get; set; }

        public DateTime UpdateDate { get; set; }

    }


    /// <summary> Вспомогательные слова - то, что должно быть отсеяно из статистики </summary>
    public interface IAuxiliaryWord
    {
        public String TheWord { get; set; }

        public DateTime InsertDate { get; set; }

    }


    /// <summary> Информация о банах </summary>
    public interface IBan
    {
        public Int32 BanId { get; set; }

        public Int32 UserId { get; set; }

        public Int32 ChatId { get; set; }

        public DateTime? BanFinishDate { get; set; }

        public DateTime UpdateDate { get; set; }

        public DateTime InsertDate { get; set; }

    }


    /// <summary> Напоминание о событиях </summary>
    public interface INotification
    {
        public Int32 NotificationId { get; set; }

        public Boolean NotificationIsActive { get; set; }

        public String NotificationMessage { get; set; }

        public Int32? NotificationMonth { get; set; }

        public Int32 NotificationDay { get; set; }

        public Int32 NotificationHour { get; set; }

        public Int32 NotificationMinute { get; set; }

        public DateTime? NotificationExecDate { get; set; }

        public DateTime UpdateDate { get; set; }

        public DateTime InsertDate { get; set; }

    }

    #endregion
    #region Classes zl

    /// <summary> Информация о времени начала учёта сообщений каждого отдельного пользователя </summary>
    [Table("accountings", Schema = "zl")]
    public partial class DbAccounting : IAccounting
    {
        [Key]
        [Required(ErrorMessage = "Property 'AccountingId' is required")]
        [Column("accounting_id", TypeName = "integer")]
        public Int32 AccountingId { get; set; }

        [Required(ErrorMessage = "Property 'AccountingStartDate' is required")]
        [Column("accounting_start_date", TypeName = "timestamp with time zone")]
        public DateTime AccountingStartDate { get; set; }

        [Required(ErrorMessage = "Property 'UpdateDate' is required")]
        [Column("update_date", TypeName = "timestamp with time zone")]
        public DateTime UpdateDate { get; set; }

    }


    /// <summary> Вспомогательные слова - то, что должно быть отсеяно из статистики </summary>
    [Table("auxiliary_words", Schema = "zl")]
    public partial class DbAuxiliaryWord : IAuxiliaryWord
    {
        [Key]
        [StringLength(100)]
        [Required(ErrorMessage = "Property 'TheWord' is required")]
        [Column("the_word", TypeName = "character varying(100)")]
        public String TheWord { get; set; }

        [Required(ErrorMessage = "Property 'InsertDate' is required")]
        [Column("insert_date", TypeName = "timestamp with time zone")]
        public DateTime InsertDate { get; set; }

    }


    /// <summary> Информация о банах </summary>
    [Table("bans", Schema = "zl")]
    public partial class DbBan : IBan
    {
        [Key]
        [Required(ErrorMessage = "Property 'BanId' is required")]
        [Column("ban_id", TypeName = "integer")]
        public Int32 BanId { get; set; }

        [Required(ErrorMessage = "Property 'UserId' is required")]
        [Column("user_id", TypeName = "integer")]
        public Int32 UserId { get; set; }

        [Required(ErrorMessage = "Property 'ChatId' is required")]
        [Column("chat_id", TypeName = "integer")]
        public Int32 ChatId { get; set; }

        [Column("ban_finish_date", TypeName = "timestamp with time zone")]
        public DateTime? BanFinishDate { get; set; }

        [Required(ErrorMessage = "Property 'UpdateDate' is required")]
        [Column("update_date", TypeName = "timestamp with time zone")]
        public DateTime UpdateDate { get; set; }

        [Required(ErrorMessage = "Property 'InsertDate' is required")]
        [Column("insert_date", TypeName = "timestamp with time zone")]
        public DateTime InsertDate { get; set; }

    }


    /// <summary> Напоминание о событиях </summary>
    [Table("notifications", Schema = "zl")]
    public partial class DbNotification : INotification
    {
        [Key]
        [Required(ErrorMessage = "Property 'NotificationId' is required")]
        [Column("notification_id", TypeName = "integer")]
        public Int32 NotificationId { get; set; }

        [Required(ErrorMessage = "Property 'NotificationIsActive' is required")]
        [Column("notification_is_active", TypeName = "boolean")]
        public Boolean NotificationIsActive { get; set; }

        [StringLength(2000)]
        [Required(ErrorMessage = "Property 'NotificationMessage' is required")]
        [Column("notification_message", TypeName = "character varying(2000)")]
        public String NotificationMessage { get; set; }

        [Column("notification_month", TypeName = "integer")]
        public Int32? NotificationMonth { get; set; }

        [Required(ErrorMessage = "Property 'NotificationDay' is required")]
        [Column("notification_day", TypeName = "integer")]
        public Int32 NotificationDay { get; set; }

        [Required(ErrorMessage = "Property 'NotificationHour' is required")]
        [Column("notification_hour", TypeName = "integer")]
        public Int32 NotificationHour { get; set; }

        [Required(ErrorMessage = "Property 'NotificationMinute' is required")]
        [Column("notification_minute", TypeName = "integer")]
        public Int32 NotificationMinute { get; set; }

        [Column("notification_exec_date", TypeName = "timestamp with time zone")]
        public DateTime? NotificationExecDate { get; set; }

        [Required(ErrorMessage = "Property 'UpdateDate' is required")]
        [Column("update_date", TypeName = "timestamp with time zone")]
        public DateTime UpdateDate { get; set; }

        [Required(ErrorMessage = "Property 'InsertDate' is required")]
        [Column("insert_date", TypeName = "timestamp with time zone")]
        public DateTime InsertDate { get; set; }

    }

    #endregion

    public partial class ChatAdminDbContext : DbContext
    {
        public DbSet<DbAccounting> Accountings { get; set; }
        public DbSet<DbAuxiliaryWord> AuxiliaryWords { get; set; }
        public DbSet<DbBan> Bans { get; set; }
        public DbSet<DbNotification> Notifications { get; set; }
    }

}