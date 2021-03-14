using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Zs.Common.Services.Logging.Seq
{
    internal class SeqEvent
    {
        public DateTime Date { get; init; }
        public List<string> Properties { get; init; }
        public List<string> Messages { get; init; }
        public string Level { get; set; }
        public string LinkPart { get; set; }
    }
}
