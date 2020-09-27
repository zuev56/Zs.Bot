﻿using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Zs.Service.ChatAdmin.Abstractions;

namespace Zs.Service.ChatAdmin.Model
{
    /// <summary> Вспомогательные слова - то, что должно быть отсеяно из статистики </summary>
    [Table("auxiliary_words", Schema = "zl")]
    public partial class AuxiliaryWord : IAuxiliaryWord
    {
        [Key]
        [StringLength(100)]
        [Required(ErrorMessage = "Property 'TheWord' is required")]
        [Column("the_word", TypeName = "character varying(100)")]
        public string TheWord { get; set; }

        [Required(ErrorMessage = "Property 'InsertDate' is required")]
        [Column("insert_date", TypeName = "timestamp with time zone")]
        public DateTime InsertDate { get; set; }
    }

}