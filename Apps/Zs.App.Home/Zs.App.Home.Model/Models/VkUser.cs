using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Zs.Bot.Data.Abstractions;

namespace Zs.App.Home.Model
{
    /// <summary> Vk user </summary>
    [Table("users", Schema = "vk")]
    public partial class VkUser : IDbEntity<VkUser, int>
    {
        [Key]
        [Required(ErrorMessage = "Property 'UserId' is required")]
        [Column("user_id", TypeName = "integer")]
        public int Id { get; set; }

        [StringLength(50)]
        [Column("first_name", TypeName = "character varying(50)")]
        public string FirstName { get; set; }

        [StringLength(50)]
        [Column("last_name", TypeName = "character varying(50)")]
        public string LastName { get; set; }

        [Required(ErrorMessage = "Property 'RawData' is required")]
        [Column("raw_data", TypeName = "json")]
        public string RawData { get; set; }

        [Required(ErrorMessage = "Property 'UpdateDate' is required")]
        [Column("update_date", TypeName = "timestamp with time zone")]
        public DateTime UpdateDate { get; set; }

        [Required(ErrorMessage = "Property 'InsertDate' is required")]
        [Column("insert_date", TypeName = "timestamp with time zone")]
        public DateTime InsertDate { get; set; }

        public Func<VkUser> GetItemToSave => () => this;
        public Func<VkUser, VkUser> GetItemToUpdate => (existingItem) => this;
    }
}
