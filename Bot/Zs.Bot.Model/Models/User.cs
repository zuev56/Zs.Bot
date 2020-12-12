using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Zs.Bot.Data.Abstractions;

namespace Zs.Bot.Data.Models
{
    [Table("users", Schema = "bot")]
    public class User : IDbEntityWithRawData<User, int>
    {
        [Key]
        [Required(ErrorMessage = "Property 'UserId' is required")]
        [Column("user_id", TypeName = "int")]
        public int Id { get; set; }

        [StringLength(50)]
        [Column("user_name", TypeName = "character varying(50)")]
        public string Name { get; set; }

        [StringLength(50)]
        [Column("user_full_name", TypeName = "character varying(50)")]
        public string FullName { get; set; }

        [StringLength(10)]
        [Required(ErrorMessage = "Property 'UserRoleId' is required")]
        [Column("user_role_code", TypeName = "character varying(10)")]
        public string UserRoleId { get; set; }

        [Required(ErrorMessage = "Property 'UserIsBot' is required")]
        [Column("user_is_bot", TypeName = "bool")]
        public bool IsBot { get; set; }

        [Required(ErrorMessage = "Property 'RawData' is required")]
        [Column("raw_data", TypeName = "json")]
        public string RawData { get; set; }

        [StringLength(50)]
        [Required(ErrorMessage = "Property 'RawDataHash' is required")]
        [Column("raw_data_hash", TypeName = "character varying(50)")]
        public string RawDataHash { get; set; }

        [Column("raw_data_history", TypeName = "json")]
        public string RawDataHistory { get; set; }

        [Required(ErrorMessage = "Property 'UpdateDate' is required")]
        [Column("update_date", TypeName = "timestamp with time zone")]
        public DateTime UpdateDate { get; set; }

        [Required(ErrorMessage = "Property 'InsertDate' is required")]
        [Column("insert_date", TypeName = "timestamp with time zone")]
        public DateTime InsertDate { get; set; }

        public ICollection<Message> Messages { get; set; }
        public UserRole UserRoles { get; set; }

        public Func<User> GetItemToSave => () => this;
        public Func<User, User> GetItemToUpdate => (existingItem) =>
        {
            return new User
            {
                Id             = existingItem.Id,
                Name           = this.Name,
                FullName       = this.FullName,
                UserRoleId     = existingItem.UserRoleId,
                IsBot          = existingItem.IsBot,
                RawData        = this.RawData,
                RawDataHash    = this.RawDataHash,
                RawDataHistory = this.RawDataHistory,
                UpdateDate     = DateTime.Now,
                InsertDate     = existingItem.InsertDate
            };
        };

        public override bool Equals(object obj)
        {
            return Equals(obj as User);
        }

        public bool Equals(User other)
        {
            return other != null &&
                   Id == other.Id &&
                   Name == other.Name &&
                   FullName == other.FullName &&
                   UserRoleId == other.UserRoleId &&
                   IsBot == other.IsBot &&
                   RawData == other.RawData &&
                   RawDataHash == other.RawDataHash &&
                   RawDataHistory == other.RawDataHistory;
        }

        public override int GetHashCode()
        {
            HashCode hash = new HashCode();
            hash.Add(Id);
            hash.Add(Name);
            hash.Add(FullName);
            hash.Add(UserRoleId);
            hash.Add(IsBot);
            hash.Add(RawData);
            hash.Add(RawDataHash);
            hash.Add(RawDataHistory);
            return hash.ToHashCode();
        }
    }
}
