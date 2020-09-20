using System.ComponentModel.DataAnnotations;

namespace Zs.Bot.Model.Db
{
    /// <summary> SQL-query result. Not a table </summary>
    public partial class DbQuery
    {
        [Key]
        public string Result { get; set; }
    }
}
