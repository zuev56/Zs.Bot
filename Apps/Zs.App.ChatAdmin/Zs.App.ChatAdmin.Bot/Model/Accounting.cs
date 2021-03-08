using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using Zs.App.ChatAdmin.Abstractions;
using Zs.Bot.Data.Abstractions;

namespace Zs.App.ChatAdmin.Model
{
    /// <summary> <inheritdoc/> </summary>
    [Table("accountings", Schema = "zl")]
    public partial class Accounting : IDbEntity<Accounting, int>
    {
        [Key]
        [Required(ErrorMessage = "Property 'AccountingId' is required")]
        [Column("accounting_id", TypeName = "integer")]
        public int Id { get; set; }

        [Required(ErrorMessage = "Property 'AccountingStartDate' is required")]
        [Column("accounting_start_date", TypeName = "timestamp with time zone")]
        public DateTime StartDate { get; set; }

        [Required(ErrorMessage = "Property 'UpdateDate' is required")]
        [Column("update_date", TypeName = "timestamp with time zone")]
        public DateTime UpdateDate { get; set; }

        [JsonIgnore]
        public Func<Accounting> GetItemToSave => () => this;
        [JsonIgnore]
        public Func<Accounting, Accounting> GetItemToUpdate => (existingItem) => this;
    }

}