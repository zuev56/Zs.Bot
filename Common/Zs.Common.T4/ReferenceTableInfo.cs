using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Zs.Common.T4
{
    /// <summary> Класс для хранения данных о таблице-справочнике </summary>
    class ReferenceTableInfo
    {
        public string Schema { get; set; }
        public string Table { get; set; }
        public string KeyColumn { get; set; }
        public string ValueColumn { get; set; }
    }
}
