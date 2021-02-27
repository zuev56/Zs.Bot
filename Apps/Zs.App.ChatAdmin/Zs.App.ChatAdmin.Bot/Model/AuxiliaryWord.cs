using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using Zs.App.ChatAdmin.Abstractions;
using Zs.Bot.Data.Abstractions;

namespace Zs.App.ChatAdmin.Model
{
    /// <summary> Вспомогательные слова - то, что должно быть отсеяно из статистики </summary>
    [Table("auxiliary_words", Schema = "zl")]
    public partial class AuxiliaryWord //: IDbEntity<AuxiliaryWord, int>
    {
        //public int Id { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        [Key]
        [StringLength(100)]
        [Required(ErrorMessage = "Property 'TheWord' is required")]
        [Column("the_word", TypeName = "character varying(100)")]
        public string TheWord { get; set; }

        [Required(ErrorMessage = "Property 'InsertDate' is required")]
        [Column("insert_date", TypeName = "timestamp with time zone")]
        public DateTime InsertDate { get; set; }

        [JsonIgnore]
        public Func<AuxiliaryWord> GetItemToSave => () => this;
        [JsonIgnore]
        public Func<AuxiliaryWord, AuxiliaryWord> GetItemToUpdate => (existingItem) => this;

    }

}