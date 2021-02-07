using System.Collections.Generic;

namespace Zs.App.Home.Data.Models.Vk
{
    /// <summary> <inheritdoc/> </summary>
    public partial class ActivityLogPage
    {
        public ushort Page { get; set; }
        public List<ActivityLogItem> Items { get; set; }
    }
}
