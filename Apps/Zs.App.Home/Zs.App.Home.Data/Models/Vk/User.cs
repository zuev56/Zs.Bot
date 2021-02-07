using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using Zs.Bot.Data.Abstractions;

namespace Zs.App.Home.Data.Models.Vk
{
    /// <summary> Vk user </summary>
    [Table("users", Schema = "vk")]
    public partial class User : IDbEntity<User, int>
    {
        [Key]
        [Column("user_id", TypeName = "integer")]
        public int Id { get; set; }

        [StringLength(50)]
        [Column("first_name", TypeName = "character varying(50)")]
        public string FirstName { get; set; }

        [StringLength(50)]
        [Column("last_name", TypeName = "character varying(50)")]
        public string LastName { get; set; }

        [Column("raw_data", TypeName = "json")]
        public string RawData { get; set; }

        [Column("update_date", TypeName = "timestamp with time zone")]
        public DateTime UpdateDate { get; set; }

        [Column("insert_date", TypeName = "timestamp with time zone")]
        public DateTime InsertDate { get; set; }
        [JsonIgnore]
        public Func<User> GetItemToSave => () => this;
        [JsonIgnore]
        public Func<User, User> GetItemToUpdate => (existingItem) => this;
    }
}
