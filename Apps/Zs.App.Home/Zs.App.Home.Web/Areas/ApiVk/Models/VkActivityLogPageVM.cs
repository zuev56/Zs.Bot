using System.Collections.Generic;

namespace Zs.App.Home.Web.Areas.ApiVk.Models
{
    /// <summary> <inheritdoc/> </summary>
    public partial class VkActivityLogPageVM
    {
        public ushort Page { get; set; }
        public List<VkActivityLogItemVM> Items { get; set; }
    }
}
