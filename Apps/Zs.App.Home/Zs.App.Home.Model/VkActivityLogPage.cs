using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Zs.App.Home.Model.Abstractions;

namespace Zs.App.Home.Model
{
    /// <summary> <inheritdoc/> </summary>
    public partial class VkActivityLogPage
    {
        public ushort Page { get; set; }
        public List<VkActivityLogItem> Items { get; set; }
    }
}
