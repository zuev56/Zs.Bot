using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Zs.Bot.Data.Abstractions;

namespace Zs.Bot.Data.Models
{
    [Table("user_roles", Schema = "bot")]
    public class UserRole : IDbEntity<UserRole, string>
    {
        [Key]
        [StringLength(10)]
        [Required(ErrorMessage = "Property 'Id' is required")]
        [Column("user_role_code", TypeName = "character varying(10)")]
        public string Id { get; set; }

        [StringLength(50)]
        [Required(ErrorMessage = "Property 'UserRoleName' is required")]
        [Column("user_role_name", TypeName = "character varying(50)")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Property 'UserRolePermissions' is required")]
        [Column("user_role_permissions", TypeName = "json")]
        public string Permissions { get; set; }

        [Required(ErrorMessage = "Property 'UpdateDate' is required")]
        [Column("update_date", TypeName = "timestamp with time zone")]
        public DateTime UpdateDate { get; set; }

        [Required(ErrorMessage = "Property 'InsertDate' is required")]
        [Column("insert_date", TypeName = "timestamp with time zone")]
        public DateTime InsertDate { get; set; }

        public Func<UserRole> GetItemToSave => () => this;
        public Func<UserRole, UserRole> GetItemToUpdate => (existingItem) => this;

        public ICollection<User> Users { get; set; }
    }

}
