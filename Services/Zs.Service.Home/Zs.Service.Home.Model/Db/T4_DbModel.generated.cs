using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Zs.Service.Home.Model.Db
{
    #region Interfaces for 'vk' schema

    /// <summary> Vk users activity log </summary>
    public interface IActivityLog
    {
        public Int32 ActivityLogId { get; set; }

        public Int32 UserId { get; set; }

        public Boolean? IsOnline { get; set; }

        public DateTime InsertDate { get; set; }

        IActivityLog DeepCopy();
    }


    /// <summary> Vk users </summary>
    public interface IUser
    {
        public Int32 UserId { get; set; }

        public String FirstName { get; set; }

        public String LastName { get; set; }

        public String RawData { get; set; }

        public DateTime UpdateDate { get; set; }

        public DateTime InsertDate { get; set; }

        IUser DeepCopy();
    }

    #endregion
    #region Classes for 'vk' schema

    /// <summary> Vk users activity log </summary>
    [Table("activity_log", Schema = "vk")]
    public partial class DbVkActivityLog : IActivityLog
    {
        [Key]
        [Required(ErrorMessage = "Property 'ActivityLogId' is required")]
        [Column("activity_log_id", TypeName = "integer")]
        public Int32 ActivityLogId { get; set; }

        [Required(ErrorMessage = "Property 'UserId' is required")]
        [Column("user_id", TypeName = "integer")]
        public Int32 UserId { get; set; }

        [Column("is_online", TypeName = "boolean")]
        public Boolean? IsOnline { get; set; }

        [Required(ErrorMessage = "Property 'InsertDate' is required")]
        [Column("insert_date", TypeName = "timestamp with time zone")]
        public DateTime InsertDate { get; set; }


        public IActivityLog DeepCopy()
        {
            return new DbVkActivityLog
            {
                ActivityLogId = this.ActivityLogId,
                UserId = this.UserId,
                IsOnline = this.IsOnline,
                InsertDate = this.InsertDate,
            };
        }
    }


    /// <summary> Vk users </summary>
    [Table("users", Schema = "vk")]
    public partial class DbVkUser : IUser
    {
        [Key]
        [Required(ErrorMessage = "Property 'UserId' is required")]
        [Column("user_id", TypeName = "integer")]
        public Int32 UserId { get; set; }

        [StringLength(50)]
        [Column("first_name", TypeName = "character varying(50)")]
        public String FirstName { get; set; }

        [StringLength(50)]
        [Column("last_name", TypeName = "character varying(50)")]
        public String LastName { get; set; }

        [Required(ErrorMessage = "Property 'RawData' is required")]
        [Column("raw_data", TypeName = "json")]
        public String RawData { get; set; }

        [Required(ErrorMessage = "Property 'UpdateDate' is required")]
        [Column("update_date", TypeName = "timestamp with time zone")]
        public DateTime UpdateDate { get; set; }

        [Required(ErrorMessage = "Property 'InsertDate' is required")]
        [Column("insert_date", TypeName = "timestamp with time zone")]
        public DateTime InsertDate { get; set; }


        public IUser DeepCopy()
        {
            return new DbVkUser
            {
                UserId = this.UserId,
                FirstName = this.FirstName,
                LastName = this.LastName,
                RawData = this.RawData,
                UpdateDate = this.UpdateDate,
                InsertDate = this.InsertDate,
            };
        }
    }

    #endregion

    public partial class HomeDbContext : DbContext
    {
        public DbSet<DbVkActivityLog> ActivityLog { get; set; }
        public DbSet<DbVkUser> Users { get; set; }
    }

}