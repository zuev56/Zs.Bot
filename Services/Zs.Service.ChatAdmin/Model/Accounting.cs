using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Zs.Service.ChatAdmin.Abstractions;

namespace Zs.Service.ChatAdmin.Model
{
    /// <summary> <inheritdoc/> </summary>
    [Table("accountings", Schema = "zl")]
    public partial class Accounting : IAccounting
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
    }

}